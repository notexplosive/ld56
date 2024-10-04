using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExplogineCore;
using ExplogineCore.Aseprite;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace LD56.CartridgeManagement;

public class LdResourceAssets
{
    private static LdResourceAssets? instanceImpl;

    private readonly Dictionary<string, Canvas> _dynamicTextures = new();

    public static LdResourceAssets Instance
    {
        get
        {
            if (instanceImpl == null)
            {
                instanceImpl = new LdResourceAssets();
            }

            return instanceImpl;
        }
    }

    public Dictionary<string, SpriteSheet> Sheets { get; } = new();
    public Dictionary<string, SoundEffectInstance> SoundInstances { get; set; } = new();
    public Dictionary<string, SoundEffect> SoundEffects { get; set; } = new();

    public IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        var resourceFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource");
        yield return new VoidLoadEvent("sprite-atlas", "Sprite Atlas", () =>
        {
            var texturePath = Path.Join(resourceFiles.GetCurrentDirectory(), "atlas.png");
            var texture = Texture2D.FromFile(Client.Graphics.Device, texturePath);
            var sheetInfo = JsonConvert.DeserializeObject<AsepriteSheetData>(resourceFiles.ReadFile("atlas.json"));

            if (sheetInfo != null)
            {
                foreach (var frame in sheetInfo.Frames)
                {
                    // Remove extension
                    var splitSheetName = 
                        frame.Key
                            .Replace(".aseprite", "")
                            .Replace(".ase", "")
                            .Replace(".png", "")
                        .Split(" ").ToList();

                    if (splitSheetName.Count > 1)
                    {
                        // If there is a number suffix, remove it
                        splitSheetName.RemoveAt(splitSheetName.Count - 1);
                    }

                    var sheetName = string.Join(" ", splitSheetName);
                    if (!Sheets.ContainsKey(sheetName))
                    {
                        Sheets.Add(sheetName, new SelectFrameSpriteSheet(texture));
                    }

                    var rect = frame.Value.Frame;
                    (Sheets[sheetName] as SelectFrameSpriteSheet)!.AddFrame(new Rectangle(rect.X, rect.Y, rect.Width,
                        rect.Height));
                }
            }
        });

        yield return new VoidLoadEvent("Sound", () =>
        {
            foreach (var path in resourceFiles.GetFilesAt(".", "ogg"))
            {
                AddSound(resourceFiles, path.RemoveFileExtension());
            }
        });
    }

    public void Unload()
    {
        Unload(_dynamicTextures);
        Unload(SoundEffects);
        Unload(SoundInstances);
    }

    private void Unload<T>(Dictionary<string,T> dictionary) where T : IDisposable
    {
        foreach (var sound in dictionary.Values)
        {
            sound.Dispose();
        }
        dictionary.Clear();
    }

    public void AddDynamicSpriteSheet(string key, Point size, Action generateTexture,
        Func<Texture2D, SpriteSheet> generateSpriteSheet)
    {
        if (_dynamicTextures.ContainsKey(key))
        {
            _dynamicTextures[key].Dispose();
            _dynamicTextures.Remove(key);
        }

        var canvas = new Canvas(size.X, size.Y);
        _dynamicTextures.Add(key, canvas);

        Client.Graphics.PushCanvas(canvas);
        generateTexture();
        Client.Graphics.PopCanvas();

        Sheets.Add(key, generateSpriteSheet(canvas.Texture));
    }

    public void AddSound(IFileSystem resourceFiles, string path)
    {
        var vorbis = ReadOgg.ReadVorbis(Path.Join(resourceFiles.GetCurrentDirectory(), path + ".ogg"));
        var soundEffect = ReadOgg.ReadSoundEffect(vorbis);
        SoundInstances[path] = soundEffect.CreateInstance();
        SoundEffects[path] = soundEffect;
    }

    public void PlaySound(string key, SoundEffectSettings settings)
    {
        if (SoundInstances.TryGetValue(key, out var sound))
        {
            if (settings.Cached)
            {
                sound.Stop();
            }

            sound.Pan = settings.Pan;
            sound.Pitch = settings.Pitch;
            sound.Volume = settings.Volume;
            sound.IsLooped = settings.Loop;

            sound.Play();
        }
        else
        {
            Client.Debug.LogWarning($"Could not find sound `{key}`");
        }
    }

    public static void Reset()
    {
        Instance.Unload();
        instanceImpl = null;
    }
}
