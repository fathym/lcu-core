using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[DataContract]
	public class PassportIdentifier : IEquatable<PassportIdentifier>
	{
		#region Properties
		[DataMember]
		public virtual string Identifier { get; set; }

		[DataMember]
		public virtual Guid ProviderID { get; set; }
		#endregion

		#region API Methods
		public bool Equals(PassportIdentifier other)
		{
			if (other == null)
				return false;

			return Identifier == other.Identifier && ProviderID == other.ProviderID;
		}

		public override bool Equals(object other)
		{
			var pi = other.As<PassportIdentifier>();

			if (pi != null)
				return Equals(pi);

			return false;
		}

		public override int GetHashCode()
		{
			return $"{ProviderID}|{Identifier}".GetHashCode();
		}
		#endregion
	}
}
