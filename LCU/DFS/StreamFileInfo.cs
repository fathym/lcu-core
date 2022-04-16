using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCU.DFS
{
    public class StreamFileInfo : IFileInfo
    {
        #region Fields
        protected readonly Stream stream;
        #endregion

        #region Properties
        public virtual bool Exists { get { return true; } }

        public virtual bool IsDirectory { get { return false; } }

        public virtual DateTimeOffset LastModified { get { return DateTime.Now; } }

        public virtual long Length { get { return stream.Length; } }

        public virtual string Name { get; protected set; }

        public virtual string PhysicalPath { get { return Name; } }
        #endregion

        #region Constructors
        public StreamFileInfo(string name, byte[] bytes)
            : this(name, new MemoryStream(bytes))
        { }

        public StreamFileInfo(string name, Stream stream)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }
        #endregion

        #region API Methods
        public virtual Stream CreateReadStream()
        {
            return stream;
        }
        #endregion
    }
}
