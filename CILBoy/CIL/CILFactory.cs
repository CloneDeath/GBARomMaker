using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CILBoy.CIL;

public class CILFactory(string root) : IDisposable {
	private List<CILAssemblyFactory> _factories = new();

	public void Dispose() {
		foreach (var factory in _factories) {
			factory.Dispose();		    
		}
	}

	public CILAssemblyFactory GetAssemblyFactoryFor(string assembly) {
		var existing = _factories.FirstOrDefault(f => f.AssemblyName == assembly);
		if (existing != null) {
			return existing;
		}
		
		var fullPath = Path.Combine(root, assembly);
		if (File.Exists($"{fullPath}.dll")) {
			var factory = new CILAssemblyFactory(this, $"{fullPath}.dll");
			_factories.Add(factory);
			return factory;
		}

		if (File.Exists($"{fullPath}.exe")) {
			var factory = new CILAssemblyFactory(this, $"{fullPath}.exe");
			_factories.Add(factory);
			return factory;
		}
		
		throw new Exception($"Could not find assembly file for {assembly}");
	}
}
