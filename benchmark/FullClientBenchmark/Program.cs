using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FatClient;

var summary = BenchmarkRunner.Run<Benchmark>();

[SimpleJob(RuntimeMoniker.Net461)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class Benchmark
{
    [Benchmark(Baseline = true)]
    public ITelegramBotClient Bench_Orig() => new TelegramBotClient("123456:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy");

    [Benchmark]
    public ITelegramBotClient Bench_Span() => new TelegramBotClientSpan("123456:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy");
}