using System;

unsafe
{
    *(ushort*)0x04000000 = 0x0403;
	try {
		throw new Exception("Error");
	}
	catch (Exception e) {
		Console.WriteLine(e.Message);
    	*(ushort*)0x06000000 = 0x03E0;
	}
}
