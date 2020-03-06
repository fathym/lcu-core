using Fathym;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Reporting
{
	public interface IReportingGraph
	{
		Task<PowerBIConfig> GetPowerBIConfig(string clientId);

		Task<Status> SavePowerBIConfig(PowerBIConfig config);
	}
}
