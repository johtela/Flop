namespace Flop.Testbench
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Flop.Testbench.Collections;
	using Flop.Testing;

	class Runner
	{
		public static VisualConsole VConsole = new VisualConsole ();

		[STAThread]
		public static void Main (string[] args)
		{
			Task.Factory.StartNew (() =>
				Tester.RunTestsTimed (
					new IStreamTests (),
					new ISequenceTests (),
					new StrictListTests (),
					new LazyListTests (),
					new MapTests (),
					new SetTests (),
					new FingerTreeTests (),
					new OptionTests (),
					new ParserMonadTests ())
			);
			Application.Run (VConsole);
			VConsole.Dispose ();
		}
	}
}
