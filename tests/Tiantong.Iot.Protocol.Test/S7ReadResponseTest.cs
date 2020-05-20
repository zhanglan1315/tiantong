using NUnit.Framework;

namespace Tiantong.Iot.Protocol.Test
{
  [TestFixture]
  public class S7ReadRedsponseTest
  {
    [Test]
    public void TestString()
    {
      var message = new byte[] {
        0x03, 0x00, 0x00, 0x23, 0x02, 0xF0, 0x80,
        0x32, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x0E, 0x01, 0x00, 0x04, 0x01,
        0xFF, 0x04, 0x00, 0x50, 0x09, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x00, 0x00, 0x00
      };
      var res = new S7ReadResponse();

      res.UseString(9);
      res.Message = message;
      Assert.AreEqual(res.IsDataResponse, true);
      Assert.AreEqual(res.ErrorCode, new byte[] { 0x01, 0x00 });
      Assert.AreEqual(res.Data, new byte[] { 0x09, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 });
      Assert.AreEqual(res.GetString(), "123456789");
    }

    [Test]
    public void TestInt()
    {
      var message = new byte[] {
        0x03, 0x00, 0x00, 0x1D, 0x02, 0xF0, 0x80,
        0x32, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0x04, 0x01,
        0xFF, 0x04, 0x00, 0x20, 0x30, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };
      var res = new S7ReadResponse();

      res.UseUInt16();
      res.Message = message;
      Assert.AreEqual(12345, res.GetUInt16());
    }

    // [Test]
    // public void TestBool()
    // {
    //   var message = new byte[] {
    //     0x03, 0x00, 0x00, 0x1A, 0x02, 0xF0, 0x80,
    //     0x32, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x05, 0x00, 0x00, 0x04, 0x01,
    //     0xFF, 0x04, 0x00, 0x08, 0x01, 0xF0, 0x00
    //   };
    //   var res = new S7ReadResponse();

    //   res.Message = message;
    //   Assert.AreEqual(res.GetBool(), true);
    // }
  } 
}
