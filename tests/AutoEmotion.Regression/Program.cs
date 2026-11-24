using AutoEmotion.Hooks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static void Reset()
{
    Svc.Hook = new();
    Svc.Objects = new();
    Svc.Log = new();
}

Reset();
Svc.Hook.ThrowOnCreate = true;
using (var reader = new EmoteReaderHooks())
    Check(!reader.IsAvailable && Svc.Log.Errors == 1, "Missing signature must disable reactions without failing startup");

Reset();
Svc.Hook.ThrowOnEnable = true;
using (var reader = new EmoteReaderHooks())
    Check(!reader.IsAvailable && Svc.Hook.Created!.IsDisposed, "Failed enable must dispose the partial hook");

Reset();
var local = new Player((nint)1, 10);
var other = new Player((nint)2, 20);
Svc.Objects.LocalPlayer = local;
Svc.Objects.Add(other);
var hook = new EmoteReaderHooks();
Check(hook.IsAvailable, "Successful hook should be available");
var events = 0;
hook.OnEmote += (_, emote) => { Check(emote == 42, "Emote ID changed"); events++; };
Svc.Hook.Detour!(0, 2, 42, 10, 0);
Check(events == 1 && Svc.Hook.OriginalCalls == 1, "Targeted emote must notify and call original once");
Svc.Hook.Detour(0, 2, 42, 99, 0);
Check(events == 1 && Svc.Hook.OriginalCalls == 2, "Unrelated target must not trigger a reaction");
Svc.Hook.Detour(0, 999, 42, 10, 0);
Check(events == 1 && Svc.Hook.OriginalCalls == 3, "Unknown actor must not trigger a reaction");
hook.OnEmote += (_, _) => throw new InvalidOperationException("subscriber failure");
Svc.Hook.Detour(0, 2, 42, 10, 0);
Check(Svc.Log.Errors == 1 && Svc.Hook.OriginalCalls == 4, "Subscriber failure must not skip the native original");
Svc.Objects.LocalPlayer = null;
Svc.Hook.Detour(0, 2, 42, 10, 0);
Check(Svc.Hook.OriginalCalls == 5, "Logout must still forward the original call");
hook.Dispose();
Check(Svc.Hook.Created!.IsDisposed, "Unload must dispose the hook");
Console.WriteLine("Passed 8 isolated hook regression scenarios. No in-game testing performed.");

sealed record Player(nint Address, ulong GameObjectId) : IPlayerCharacter;
