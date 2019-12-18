using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace PiGPIO
{
    public enum PinMode
    {
        Input = 0, Output = 1
    }

    public static class GPIO
    {
        [DllImport("libc")]
        private static extern uint getuid ();

        public static void Flush(int pins = 40)
        {
            for (int i = 0; i < pins; i++)
            {
                if (IsOpen(i))
                    Unexport(i);
            }
        }

        public static void On(int pin)
        {
            File.WriteAllText("/sys/class/gpio/gpio" + pin + "/value", "1");
        }

        public static void Off(int pin)
        {
            File.WriteAllText("/sys/class/gpio/gpio" + pin + "/value", "0");
        }

        public static void Set(int pin, bool value)
        {
            File.WriteAllText("/sys/class/gpio/gpio" + pin + "/value", value ? "1" : "0");
        }

        public static bool Get(int pin)
        {
            // This is somewhat interesting, value file has a line break at the end
            // Other solution wolud be trimming the spaces but I chose the latter for performance
            return File.ReadAllText("/sys/class/gpio/gpio" + pin + "/value") == "1\n";
        }

        public static bool IsOpen(int pin)
        {
            return Directory.Exists("/sys/class/gpio/gpio" + pin + "/");
        }

        public static PinMode GetPinMode(int pin)
        {
            if (File.ReadAllText("/sys/class/gpio/gpio" + pin + "/direction") == "out")
                return PinMode.Output;
            return PinMode.Input;
        }

        public static void SetPinMode(int pin, PinMode mode)
        {
            if (mode == PinMode.Output)
                File.WriteAllText("/sys/class/gpio/gpio" + pin + "/direction", "out");
            else
                File.WriteAllText("/sys/class/gpio/gpio" + pin + "/direction", "in");
        }

        /* So I should tell about this waitOsSleepMillis constant here
         * Raspberry is such a small computer with small computing power
         * So when something is going on at background (say, compiling) IO functions might get a littly janky, since CPU is busy compiling stuff
         * So when we export a new pin, OS might not initialize the files yet. This will create undefined behaviour.
         * If application won't wait for a while after exporting, crash WILL happen. 99 percent of the time I tried with no wait, it crashed.
         * There is two ways that we can get around this:
         *  - Running application with root privileges.
         *  - Waiting for OS to finish creating pin files
         * 2. options is selected by default.
         * You might be asking: Isn't 100 milliseconds a bit too much?
         *  No. I tried with 50ms. It works when there is no load on CPU. When I compile something and run this at the same time, it crashes.
         *  100ms seemed safe with my tests. Even if CPU is 100% loaded, this won't crash.
         * Sorry you have to sacrifice a little bit of your clock cucles to exports. Exports only.
         * After a pin is exported, it can be used as fast as you can without a delay.
         * So there will be a litte 'warmup' stage.
         * Even if you *somehow* managed to use 40 pins with this library (which is impossible with current pi models), 40 * 100 means 4 seconds of initialization for entire pins.
         * Rest you can skyrocket your pin on and off's
         * You can enable the 'root mode' with EnableRootMode method, which will return if application is running in root mode or not
         * This will skip the sleep period of the export functions, which they will be as fast as they can
         * You WILL have to use return value of EnableRootMode and check if application is running in root mode, or crashes will be inevitable.
         * Most basic control you can do is: if (!GPIO.EnableRootMode()) return; // or Environment.Exit(0)
         */
        const int waitOsSleepMillis = 100;
        static bool rootMode = false;
        public static void Export(int pin, PinMode mode)
        {
            File.WriteAllText("/sys/class/gpio/export", pin.ToString());
            if (!rootMode)
                Thread.Sleep(waitOsSleepMillis);
            SetPinMode(pin, mode);
        }

        public static void Export(int pin)
        {
            File.WriteAllText("/sys/class/gpio/export", pin.ToString());
            if (!rootMode)
                Thread.Sleep(waitOsSleepMillis);
        }

        public static void Unexport(int pin)
        {
            File.WriteAllText("/sys/class/gpio/unexport", pin.ToString());
        }

        public static bool EnableRootMode()
        {
            rootMode = true;
            return getuid() == 0;
        }

        public static void DisableRootMode()
        {
            rootMode = false;
        }
    }
}