// Test-only boundaries: no game process, native code or Dalamud runtime is loaded.
namespace Dalamud.Game.ClientState.Objects.SubKinds
{
    public interface IPlayerCharacter
    {
        nint Address { get; }
        ulong GameObjectId { get; }
    }
}

namespace Dalamud.Hooking
{
    public sealed class Hook<T>(T original) : IDisposable where T : Delegate
    {
        public T Original { get; } = original;
        public bool ThrowOnEnable { get; set; }
        public bool IsDisposed { get; private set; }
        public void Enable()
        {
            if (ThrowOnEnable) throw new InvalidOperationException("enable failure");
        }
        public void Dispose() => IsDisposed = true;
    }
}

namespace ECommons.DalamudServices
{
    using AutoEmotion.Hooks;
    using Dalamud.Game.ClientState.Objects.SubKinds;
    using Dalamud.Hooking;

    public static class Svc
    {
        public static FakeInterop Hook { get; set; } = new();
        public static FakeObjects Objects { get; set; } = new();
        public static FakeLog Log { get; set; } = new();
    }
    public sealed class FakeInterop
    {
        public bool ThrowOnCreate { get; set; }
        public bool ThrowOnEnable { get; set; }
        public int OriginalCalls { get; private set; }
        public EmoteReaderHooks.OnEmoteFuncDelegate? Detour { get; private set; }
        public Hook<EmoteReaderHooks.OnEmoteFuncDelegate>? Created { get; private set; }
        public Hook<T> HookFromSignature<T>(string signature, T detour) where T : Delegate
        {
            if (ThrowOnCreate) throw new InvalidOperationException("signature missing");
            Detour = (EmoteReaderHooks.OnEmoteFuncDelegate)(Delegate)detour;
            Created = new((_, _, _, _, _) => OriginalCalls++) { ThrowOnEnable = ThrowOnEnable };
            return (Hook<T>)(object)Created;
        }
    }
    public sealed class FakeObjects : List<IPlayerCharacter>
    {
        public IPlayerCharacter? LocalPlayer { get; set; }
    }
    public sealed class FakeLog
    {
        public int Errors { get; private set; }
        public void Error(Exception exception, string message) => Errors++;
    }
}
