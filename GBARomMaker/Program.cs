using System.IO;
using GBARomMaker.Rom;
using GBARomMaker.Compilation;

namespace GBARomMaker;
public static class Program {
	public static void Main() {
		var data = File.ReadAllBytes("../../homebrew/pliko_013b.gba");
		var file = new RomFile(data);

		var newFile = new RomFile();
		newFile.Header.GameTitle = "red pixel";

		var compiler = new Compiler();

		newFile.Content = 
			compiler.GetOperationForLine("ldr r0, =0x04000000     @ Display control register").ToBytes();
			// ldr r1, =0x0403         @ Mode 3 + BG2 enabled
			// strh r1, [r0]
			// ldr r0, =0x06000000     @ Top-left pixel in VRAM
			// mov r1, #0x1F           @ Red
			// strh r1, [r0]




		File.WriteAllBytes("../../homebrew/just-header.gba", newFile.ToBytes());
	}
}
