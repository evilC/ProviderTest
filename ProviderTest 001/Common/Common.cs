using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct InputModeReport
    {
        public InputModeReport(BindingDescriptor bindingDescriptor, int value)
        {
            BindingDescriptor = bindingDescriptor;
            Value = value;
        }
        public BindingDescriptor BindingDescriptor { get; }
        public int Value { get; }
    }
}
