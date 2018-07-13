using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct InputReport
    {
        public InputReport(InputDescriptor inputDescriptor, int value)
        {
            InputDescriptor = inputDescriptor;
            Value = value;
        }
        public InputDescriptor InputDescriptor { get; }
        public int Value { get; }
    }
}
