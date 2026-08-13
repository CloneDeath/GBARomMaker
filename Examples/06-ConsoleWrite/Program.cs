using System;
using GBA;

unsafe {
	Console.WriteLine("hello world");
	Console.WriteLine(12);
	Console.WriteLine("h" + 1);
	for (var i = 0; i < 10; i++) {
		Console.WriteLine("value " + i);
		//Console.WriteLine($"\tis not {i+1}.");
	}
	DisplayController.EnableVBlank();
	while (true) {
		Interrupt.WaitVBlank();
	}
}

