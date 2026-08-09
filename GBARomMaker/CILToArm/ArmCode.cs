namespace GBARomMaker.CILToArm;

public class ArmCode {
	public ArmCode(string line) {
		Assembly = [line];
	}

	public ArmCode(string[] lines) {
		Assembly = lines;
	}

	public string[] Assembly { get; }

	public bool IncludeFloat { get; set; } = false;
	public bool IncludeSin { get; set; } = false;
	public bool IncludeMGBALog { get; set; } = false;
}
