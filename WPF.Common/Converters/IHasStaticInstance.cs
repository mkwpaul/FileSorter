using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPF.Common.Converters
{
    public interface IHasStaticInstance<out T> where T : IHasStaticInstance<T>
    {
        static abstract T Instance { get; }
    }
}
