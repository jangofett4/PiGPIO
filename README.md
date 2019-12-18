# PiGPIO

When I got my first raspberry pi (RPi Zero), I was looking for ways to use its GPIO pins. As it turns out its pretty [easy](http://codefoster.com/pi-basicgpio/).  
So I jumped into my favourite language and write this small library to access to GPIO pins easily.

## Using PiGPIO

You can compile it into a dll file and then use it or grab a pre-compiled one (available soon) or just grab the PiGPIO.cs file and start using it.  
Basic usage:  
```cs
using System;
using System.Threading;
using PiGPIO;

namespace Program
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            GPIO.Flush(); // Close any open pins ('flush' the pins)
            /* Output Direction */
            GPIO.Export(17, PinMode.Output);
            for (;;)
            {
                GPIO.On(17);
                Thread.Sleep(1000);
                GPIO.Off(17);
                Thread.Sleep(1000);
            }
            /* Input Direction */
            GPIO.Export(17, PinMode.Input);
            if (GPIO.Get(17))
            {
                Console.WriteLine("Pin 17 is HIGH");
            }
        }
    }
}
```
Check out the [wiki](https://github.com/jangofett4/PiGPIO/wiki) for more detailed instructions about how to use this library and its features.

## Compatibility
I wrote and tested this in a Raspberry Pi Zero, It works as intended. Other raspberries should work too.  
Tested with:
`Mono JIT compiler version 5.18.0.240 (Debian 5.18.0.240+dfsg-3 Sat Apr 20 05:16:08 UTC 2019)`

## Dependencies
This has NO dependecies whatsoever.  
Just one thing to mention: needs Linux. This was designed to run on linux and might not work on Windows.
