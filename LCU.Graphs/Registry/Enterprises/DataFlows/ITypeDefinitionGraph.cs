using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface ITypeDefinitionGraph
	{
        Task<List<TypeDefinition>> ListTypeDefinitions(string entLookup);

        Task<TypeDefinition> SaveTypeDefinition(string entLookup, TypeDefinition typeDefinition);
    }
}