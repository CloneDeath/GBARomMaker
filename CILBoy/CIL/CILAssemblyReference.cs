using System.Reflection.Metadata;

namespace CILBoy.CIL;

public class CILAssemblyReference(AssemblyReference assembly) {
	public string Name => assembly.GetAssemblyName().Name ?? throw new System.Exception("Null name!");
}
