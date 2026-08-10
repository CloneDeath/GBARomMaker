using System;
using GBA;

unsafe {
	Console.WriteLine("hello world!");
	Console.WriteLine(12);
	DisplayController.EnableVBlank();
	while (true) {
		Interrupt.WaitVBlank();
	}
}

