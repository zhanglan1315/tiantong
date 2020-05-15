using System.Text.Json;
using NUnit.Framework;

namespace Tiantong.Iot.Test
{
  [TestFixture]
  public class WatcherTest
  {
    [Test]
    public void TestHandleCancel()
    {
      var result = 0;
      var watcher = new Watcher<int>();
      
      watcher.When(value => value != 0).On(value => result = value);
      watcher.Emit(1);

      Assert.AreEqual(result, 1);
    }

    [Test]
    public void TestCustomEvent()
    {
      var result = 0;
      var watcher = new Watcher<WatcherUser>();
      var user = new WatcherUser() { Id = 1, Name = "test" };

      watcher.When(item => item.Id == 10).On(item => result = item.Id);

      watcher.Emit(user);
      Assert.AreEqual(result, 0);

      user.Id = 10;
      watcher.Emit(user);
      Assert.AreEqual(result, 10);
    }

    [Test]
    public void TestWhenOpt()
    {
      var flag = "";
      var watcher = new Watcher<string>();
      watcher.On(value => flag = value);

      watcher.When("", "");
      watcher.Emit("any");
      Assert.AreEqual("any", flag);

      watcher.When("=", "equal");
      watcher.Emit("equ");
      Assert.AreEqual("any", flag);
      watcher.Emit("equal");
      Assert.AreEqual("equal", flag);

      watcher.When("!=", "not");
      watcher.Emit("not");
      Assert.AreEqual("equal", flag);
      watcher.Emit("not_equal");
      Assert.AreEqual("not_equal", flag);

      watcher.When(">", "100");
      watcher.Emit("10");
      Assert.AreEqual("not_equal", flag);
      watcher.Emit("101");
      Assert.AreEqual("101", flag);

      watcher.When("<", "100");
      watcher.Emit("100");
      Assert.AreEqual("101", flag);
      watcher.Emit("99");
      Assert.AreEqual("99", flag);
    }

    [Test]
    public void TestHttpPostNumber()
    {
      var client = new TestHttpPusherClient();
      var watcher = new StateHttpPusher<int>(client);
      watcher.Post("test", "key", false, "{\"field\": 1}").When("", "");
      watcher.Emit(100);

      Assert.AreEqual(client.Url, "test");
      Assert.AreEqual(JsonDocument.Parse(client.Data).RootElement.GetProperty("key").GetInt32(), 100);
      Assert.AreEqual(JsonDocument.Parse(client.Data).RootElement.GetProperty("field").GetInt32(), 1);
    }

    [Test]
    public void TestHttpPostNumberToString()
    {
      var client = new TestHttpPusherClient();
      var watcher = new StateHttpPusher<int>(client);
      watcher.Post("test", "key", true).When("", "");
      watcher.Emit(100);

      Assert.AreEqual(client.Url, "test");
      Assert.AreEqual(JsonDocument.Parse(client.Data).RootElement.GetProperty("key").GetString(), "100");
    }

    [Test]
    public void TestHttpPostString()
    {
      var client = new TestHttpPusherClient();
      var watcher = new StateHttpPusher<string>(client);
      watcher.Post("test", "string").When("", "");
      watcher.Emit("hello world");

      Assert.AreEqual(client.Url, "test");
      Assert.AreEqual(JsonDocument.Parse(client.Data).RootElement.GetProperty("string").GetString(), "hello world");
    }

    [Test]
    public void TestHttpPostStringToString()
    {
      var client = new TestHttpPusherClient();
      var watcher = new StateHttpPusher<string>(client);
      watcher.Post("test", "string", true).When("", "");
      watcher.Emit("hello world");

      Assert.AreEqual(client.Url, "test");
      Assert.AreEqual(JsonDocument.Parse(client.Data).RootElement.GetProperty("string").GetString(), "hello world");
    }

  }
}
