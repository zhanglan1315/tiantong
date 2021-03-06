using NUnit.Framework;
using Tiantong.Iot.Entities;

namespace Tiantong.Iot.Test
{
  [TestFixture]
  public class PlcClientTest
  {
    [Test]
    public void TestClient()
    {
      var options = new PlcClientOptions(0, "name", PlcModel.Test, "", 0, null, null);

      options.State<int>().Id(1).Name("int").Address("D100");

      var client = new PlcClient(options);

      client.Connect();
      client.State(1).Set("100");

      Assert.AreEqual("100", client.State(1).Get());
    }
  }
}
