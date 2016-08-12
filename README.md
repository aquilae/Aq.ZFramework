# `Aq.ZFramework`

## Very basic web-like framework and server using ZeroMQ

### Usage

```csharp
class Program {
  class InnerMiddleware : IMiddleware {
    public async Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
      await context.ReplyAsync("I am World!");
    }
  }

  static async Task MainAsync() {
    var server = new ServerBuilder()
      .Bind("tcp://127.0.0.1:5555")
      .UseServiceProvider(services => {
          services.AddTransient<InnerMiddleware>();
      })
      .Use(async (ctx, next) => {
        Debug.Assert("Hey" == ctx.Request.First.ConvertToString());
        await ctx.ReplyAsync("Hello");
        await next();
      })
      .Use<InnerMiddleware>()
      .BuildAndStart();

    using (var client = new DealerSocket(">tcp://127.0.0.1:5555")) {
      client.SendFrame("Hey");
      Debug.Assert("Hello" == client.ReceiveFrameString());
      Debug.Assert("I am World!" == client.ReceiveFrameString());
      client.Close();
    }

    server.Complete();
    await server.Completion;
  }
}
```
