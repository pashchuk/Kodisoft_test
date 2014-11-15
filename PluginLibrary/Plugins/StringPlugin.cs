
namespace PluginLibrary.Plugins
{
	public class StringPlugin : Plugin<string>
	{
		public StringPlugin() { }
		public override string Modify(string param)
		{
			return param.ToUpper();
		}
	}
}
