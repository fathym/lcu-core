using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises
{
	public interface IEnterpriseGraph
	{
		Task<Enterprise> Create(string name, string description, string host);

		Task<bool> DoesHostExist(string host);

		Task<Enterprise> LoadByHost(string host);

		Task<Enterprise> LoadByPrimaryAPIKey(string apiKey);
	}
}
