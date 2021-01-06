using System.Threading.Tasks;
using Pulumi;

namespace pulumi_cosmos_functions
{
  public class Program
  {
    static Task<int> Main()
    {
      return Deployment.RunAsync<MyCosmosFunctionStack>();
    }
  }
}
