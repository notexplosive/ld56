using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD56.Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD56.CartridgeManagement;

public class LdCartridge(IRuntime runtime) : BasicGameCartridge(runtime)
{
    private ISession? _session;
    private EditorSession? _editorSession;
    private LdSession? _gameSession;

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.LinearWrap);

    public override void OnCartridgeStarted()
    {
        _editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);
        _gameSession = new LdSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);

        _editorSession.RequestPlay += () =>
        {
            if (Client.Debug.IsPassiveOrActive)
            {
                _session = _gameSession;
                _gameSession.World.LoadLevel(_editorSession.CurrentLevel);
            }
        };

        _gameSession.RequestEditor += () =>
        {
            if (Client.Debug.IsPassiveOrActive)
            {
                _session = _editorSession;
            }
        };

        if (Client.Debug.IsPassiveOrActive)
        {
            _session = _editorSession;
        }
        else
        {
            _session = _gameSession;
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _session?.UpdateInput(input, hitTestStack);
    }

    public override void Update(float dt)
    {
        _session?.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _session?.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override void OnHotReload()
    {
        _session?.OnHotReload();
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        LdResourceAssets.Reset();
        foreach (var item in LdResourceAssets.Instance.LoadEvents(painter))
        {
            yield return item;
        }
    }
}