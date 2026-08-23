using System;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class NEWOBJ(CILAssemblyFactory factory, ARMProgram assembly) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Newobj];

	public ArmCode Handle(InstructionMetadata instruction) {
		var newobj = (CILBoy.CILParse.Instructions.NEWOBJ)instruction.Instruction;
		var handle = MetadataTokens.EntityHandle(newobj.MetadataToken);
		var method = factory.GetMethodDefinition(handle);

		if (method.Name != ".ctor") {
			throw new Exception($"Tried to initialize an object with something that isn't a contructor: {method.FullName} -- {instruction}");
		}
		assembly.MethodsToTranspile.Enqueue(method);

		var classLayout = assembly.GetClassLayout(method.Parent);
		var target = assembly.GetLabelForMethod(method);
		if (method.ParameterCount == 0) {
			return new ArmCode([
				$"ldr r0, ={classLayout.Size * 4}",
				"bl gba_malloc",
				"push sp!, { r0 } @ push object ref onto the stack...",
				"push sp!, { r0 } @ push it again for the 'this' param of the constructor",
				$"bl {target}"
			]);
		} else if (method.ParameterCount <= 9) {
			var registers = method.ParameterCount == 1 
				? "r0"
				: $"r0-r{method.ParameterCount - 1}";
			return new ArmCode([
				$"ldr r0, ={classLayout.Size * 4}",
				"bl gba_malloc",
				"mov ip, r0",
				$"pop sp!, {{ {registers} }}",
				"push sp!, { ip } @ push object ref onto the stack...",
				"push sp!, { ip } @ push it again for the 'this' param of the constructor",
				$"push sp!, {{ {registers} }}",
				$"bl {target}"
			]);
		} else {
			throw new Exception($"Only up to 9 args are supported... {instruction}");
		}
	}
}
