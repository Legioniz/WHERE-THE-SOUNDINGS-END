using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Interaction;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>Title, pause, save/load and settings overlay for Greyhaven.</summary>
    [DisallowMultipleComponent]
    public sealed class GreyhavenFrontEndUI : MonoBehaviour
    {
        private UIDocument document;
        private InputActionAsset controls;
        private SaveGameCoordinator saves;
        public GreyhavenRuntimeUI GameplayUI { get; set; }
        public bool IsOverlayOpen => page != "game";
        private VisualElement overlay, content, root;
        private Label feedback;
        private InputAction pauseAction;
        private bool paused, firstLaunch = true;
        private float previousTimeScale = 1f;
        private string page = "title";
        private static readonly Color Ink = new Color(.035f,.065f,.067f,.98f);
        private static readonly Color Panel = new Color(.075f,.12f,.12f,1f);
        private static readonly Color Paper = new Color(.88f,.86f,.78f,1f);
        private static readonly Color Muted = new Color(.62f,.69f,.65f,1f);
        private static readonly Color Accent = new Color(.72f,.34f,.23f,1f);

        public void Initialize(UIDocument uiDocument, InputActionAsset input, SaveGameCoordinator coordinator)
        {
            document = uiDocument; controls = input; saves = coordinator;
            if (document == null || document.rootVisualElement == null) return;
            root = document.rootVisualElement;
            Screen.fullScreen = PlayerPrefs.GetInt("wse.fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            AudioListener.volume = PlayerPrefs.GetFloat("wse.masterVolume", 1f);
            root.style.scale = new Scale(Vector2.one * PlayerPrefs.GetFloat("wse.uiScale", 1f));
            BuildShell();
            InputActionMap map = controls != null ? controls.FindActionMap("Boat", false) : null;
            pauseAction = map != null ? map.FindAction("Pause", false) : null;
            if (pauseAction != null) pauseAction.performed += OnPause;
            OpenTitle();
        }

        private void OnDestroy()
        {
            if (pauseAction != null) pauseAction.performed -= OnPause;
            Unpause();
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (page == "title") return;
            if (page == "pause")
            {
                Resume();
                return;
            }

            if (page != "game" || !DocksideModalPolicy.CanOpenPauseMenu(
                GameplayUI != null && GameplayUI.DocksideHub != null && GameplayUI.DocksideHub.IsMenuOpen,
                GameplayUI != null && GameplayUI.IsInventoryOpen,
                GameplayUI != null && GameplayUI.IsDialogueOpen))
                return;

            OpenPause();
        }

        private void BuildShell()
        {
            overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0; overlay.style.right = 0; overlay.style.top = 0; overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(.01f,.025f,.03f,.93f);
            overlay.style.justifyContent = Justify.Center; overlay.style.alignItems = Align.Center;
            overlay.pickingMode = PickingMode.Position;
            root.Add(overlay);
            VisualElement frame = new VisualElement();
            frame.style.width = new Length(88,LengthUnit.Percent); frame.style.maxWidth = 900;
            frame.style.maxHeight = new Length(92,LengthUnit.Percent);
            frame.style.backgroundColor = Ink; frame.style.paddingLeft = 26; frame.style.paddingRight = 26;
            frame.style.paddingTop = 22; frame.style.paddingBottom = 20;
            frame.style.borderTopWidth = 3; frame.style.borderTopColor = Accent;
            overlay.Add(frame);
            Label eyebrow = new Label("A GREYHAVEN FIELD JOURNAL");
            eyebrow.style.color = Accent; eyebrow.style.fontSize = 10; frame.Add(eyebrow);
            Label title = new Label("WHERE THE SOUNDINGS END");
            title.style.color = Paper; title.style.fontSize = 24; title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 14; frame.Add(title);
            VisualElement line = new VisualElement(); line.style.height = 1;
            line.style.backgroundColor = new Color(.29f,.40f,.38f,1f); line.style.marginBottom = 14; frame.Add(line);
            content = new VisualElement(); content.style.flexGrow = 1; content.style.minHeight = 0; frame.Add(content);
            feedback = new Label(""); feedback.style.color = new Color(.9f,.72f,.53f);
            feedback.style.fontSize = 11; feedback.style.whiteSpace = WhiteSpace.Normal;
            feedback.style.marginTop = 12; frame.Add(feedback);
            Label footer = new Label("ESC / START  PAUSE    ·    SOME WATERS SHOULD NEVER BE DISTURBED");
            footer.style.color = Muted; footer.style.fontSize = 9; footer.style.marginTop = 12; frame.Add(footer);
        }

        private void OpenTitle()
        {
            page = "title"; Pause(); Show(); content.Clear();
            Heading("THE COAST IS WAITING.");
            Body("A quiet harbour. A missing fisherman. A sea that does not always return what it takes.");
            AddButton("BEGIN / CONTINUE", Continue, true);
            AddButton("LOAD FIELD RECORD", OpenSaves);
            AddButton("SETTINGS", OpenSettings);
            AddButton("QUIT TO DESKTOP", Application.Quit);
            Hint("Continue restores the latest manual record or separate autosave. If none exists, begin a new voyage.");
            feedback.text = "Your field records remain stored on this device.";
        }

        private void OpenPause()
        {
            page = "pause"; Pause(); Show(); content.Clear();
            Heading("FIELD PAUSED"); Body("Take a moment. The coast will still be here.");
            AddButton("RESUME VOYAGE", Resume, true);
            AddButton("SAVE / LOAD FIELD RECORD", OpenSaves);
            AddButton("SETTINGS", OpenSettings);
            AddButton("RETURN TO TITLE", OpenTitle);
            feedback.text = "The vessel and world simulation are paused.";
        }

        private void OpenSaves()
        {
            page = "saves"; Pause(); Show(); content.Clear();
            Heading("FIELD RECORDS");
            Body("Five manual slots. Autosave is separate. Cross-scene restore is not yet available.");
            List<SaveSlotInfo> slots = saves != null ? saves.GetSlots() : new List<SaveSlotInfo>();
            for (int i = 0; i < SaveFileService.MaximumSlot; i++)
            {
                int n = i + 1; SaveSlotInfo slot = i < slots.Count ? slots[i] : null;
                VisualElement row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center; row.style.backgroundColor = Panel;
                row.style.paddingLeft = 8; row.style.paddingRight = 5; row.style.paddingTop = 6;
                row.style.paddingBottom = 6; row.style.marginBottom = 5; content.Add(row);
                VisualElement info = new VisualElement(); info.style.flexGrow = 1; row.Add(info);
                Label label = new Label("RECORD " + n.ToString("00"));
                label.style.color = Paper; label.style.fontSize = 12; info.Add(label);
                Label detail = new Label(slot != null && slot.IsOccupied ? SlotDetails(slot) : "EMPTY / NO FIELD RECORD");
                detail.style.color = Muted; detail.style.fontSize = 9; detail.style.whiteSpace = WhiteSpace.Normal; info.Add(detail);
                if (slot != null && slot.IsOccupied)
                {
                    SmallButton(row,"LOAD",()=>Run(saves.LoadSlot(n),"Record restored."));
                    SmallButton(row,"SAVE",()=>Run(saves.SaveSlot(n),"Record overwritten."));
                    SmallButton(row,"DELETE",()=>ConfirmDelete(n));
                }
                else SmallButton(row,"SAVE HERE",()=>Run(saves != null ? saves.SaveSlot(n) : SaveOperationResult.Failed("Save service unavailable."),"Record saved."));
            }
            AddButton("SAVE TO NEXT EMPTY SLOT", SaveNext);
            AddButton("BACK", Back);
            feedback.text = "Save and load actions report success or failure here.";
        }

        private void OpenSettings()
        {
            page = "settings"; Pause(); Show(); content.Clear();
            Heading("VESSEL & DISPLAY SETTINGS");
            Body("Preferences are stored locally and remain separate from campaign saves.");
            Section("DISPLAY");
            AddToggle("Fullscreen",Screen.fullScreen,v=>{Screen.fullScreen=v;PlayerPrefs.SetInt("wse.fullscreen",v?1:0);});
            AddSlider("Interface scale",.85f,1.25f,PlayerPrefs.GetFloat("wse.uiScale",1f),v=>{root.style.scale=new Scale(new Vector2(v,v));PlayerPrefs.SetFloat("wse.uiScale",v);});
            Section("AUDIO");
            AddSlider("Master volume",0f,1f,PlayerPrefs.GetFloat("wse.masterVolume",1f),v=>{AudioListener.volume=v;PlayerPrefs.SetFloat("wse.masterVolume",v);});
            AddSlider("Music preference",0f,1f,PlayerPrefs.GetFloat("wse.musicVolume",.8f),v=>PlayerPrefs.SetFloat("wse.musicVolume",v));
            AddSlider("Effects preference",0f,1f,PlayerPrefs.GetFloat("wse.effectsVolume",1f),v=>PlayerPrefs.SetFloat("wse.effectsVolume",v));
            Section("ACCESSIBILITY");
            AddToggle("Reduce camera shake (preference)",PlayerPrefs.GetInt("wse.reduceCameraShake",0)==1,v=>PlayerPrefs.SetInt("wse.reduceCameraShake",v?1:0));
            AddToggle("High contrast (preference)",PlayerPrefs.GetInt("wse.highContrast",0)==1,v=>PlayerPrefs.SetInt("wse.highContrast",v?1:0));
            AddButton("RESTORE DEFAULTS",ResetSettings); AddButton("BACK",Back);
            feedback.text = "Only master volume, fullscreen, and interface scale currently have direct runtime effects.";
        }

        private void Continue()
        {
            if (saves == null) { firstLaunch=false; Resume(); return; }
            List<SaveSlotInfo> slots=saves.GetSlots(); SaveSlotInfo newest=null; DateTime latest=DateTime.MinValue;
            foreach(SaveSlotInfo slot in slots)
            {
                DateTime parsed;
                if(slot!=null&&slot.IsOccupied&&DateTime.TryParse(slot.SavedAtUtc,out parsed)&&parsed>latest){newest=slot;latest=parsed;}
            }
            SaveOperationResult result = newest!=null ? saves.LoadSlot(newest.SlotNumber) : saves.LoadAutosave();
            if(result.Success){feedback.text=result.Message;firstLaunch=false;Resume();}
            else if(newest==null){feedback.text="No previous voyage found. Starting a new voyage.";firstLaunch=false;Resume();}
            else feedback.text="LOAD FAILED: "+result.Message;
        }

        private void SaveNext()
        {
            if(saves==null){feedback.text="Save service unavailable.";return;}
            List<SaveSlotInfo> slots=saves.GetSlots();
            for(int i=0;i<slots.Count;i++)if(!slots[i].IsOccupied){Run(saves.SaveSlot(i+1),"Record saved.");return;}
            feedback.text="All five slots are occupied. Choose SAVE on a record to overwrite it.";
        }

        private void ConfirmDelete(int slot)
        {
            page="confirm";content.Clear();Heading("DELETE RECORD "+slot.ToString("00")+"?");
            Body("This permanently removes the save and its recovery backup. This cannot be undone.");
            AddButton("YES, DELETE RECORD",()=>{SaveOperationResult r=saves.DeleteSlot(slot);OpenSaves();feedback.text=r.Message;},true);
            AddButton("CANCEL",OpenSaves);
        }

        private void ResetSettings()
        {
            foreach(string key in new[]{"wse.fullscreen","wse.uiScale","wse.masterVolume","wse.musicVolume","wse.effectsVolume","wse.reduceCameraShake","wse.highContrast"})PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();Screen.fullScreen=true;AudioListener.volume=1f;root.style.scale=new Scale(Vector2.one);
            OpenSettings();feedback.text="Default preferences restored.";
        }

        private void Run(SaveOperationResult result,string success)
        {
            feedback.text=result.Success?success+" "+result.Message:"ACTION FAILED: "+result.Message;
            if(result.Success&&result.RecoveredFromBackup)feedback.text+=" Recovery backup was used.";
            if(result.Success&&page=="saves"){OpenSaves();feedback.text=success+" "+result.Message+(result.RecoveredFromBackup?" Recovery backup was used.":"");}
        }

        private void Back(){if(page=="confirm")OpenSaves();else if(firstLaunch)OpenTitle();else OpenPause();}
        private void Resume(){firstLaunch=false;page="game";overlay.style.display=DisplayStyle.None;Unpause();}
        private void Pause(){if(paused)return;previousTimeScale=Time.timeScale;Time.timeScale=0f;paused=true;}
        private void Unpause(){if(!paused)return;Time.timeScale=previousTimeScale>0f?previousTimeScale:1f;paused=false;}
        private void Show(){overlay.style.display=DisplayStyle.Flex;}

        private static string SlotDetails(SaveSlotInfo s)
        {
            DateTime date;string stamp=DateTime.TryParse(s.SavedAtUtc,out date)?date.ToLocalTime().ToString("yyyy-MM-dd HH:mm"):"DATE UNKNOWN";
            TimeSpan play=TimeSpan.FromSeconds(Math.Max(0,s.TotalPlaytimeSeconds));
            return (string.IsNullOrEmpty(s.SceneId)?"UNKNOWN AREA":s.SceneId)+" · "+stamp+" · "+play.ToString(@"hh\:mm")+(s.RecoveredFromBackup?" · BACKUP":"");
        }
        private void Heading(string t){Label l=new Label(t);l.style.color=Paper;l.style.fontSize=19;l.style.unityFontStyleAndWeight=FontStyle.Bold;l.style.marginBottom=6;content.Add(l);}
        private void Body(string t){Label l=new Label(t);l.style.color=Muted;l.style.fontSize=11;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginBottom=12;content.Add(l);}
        private void Hint(string t){Label l=new Label(t);l.style.color=Muted;l.style.fontSize=9;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginTop=8;content.Add(l);}
        private void Section(string t){Label l=new Label(t);l.style.color=Accent;l.style.fontSize=10;l.style.marginTop=7;content.Add(l);}
        private void AddButton(string t,Action a,bool primary=false){Button b=new Button(a){text=t};b.style.height=34;b.style.marginBottom=5;b.style.paddingLeft=11;b.style.backgroundColor=primary?Accent:Panel;b.style.color=Paper;b.style.unityTextAlign=TextAnchor.MiddleLeft;content.Add(b);}
        private void SmallButton(VisualElement p,string t,Action a){Button b=new Button(a){text=t};b.style.minWidth=48;b.style.height=26;b.style.marginLeft=3;b.style.fontSize=9;b.style.backgroundColor=Panel;b.style.color=Paper;p.Add(b);}
        private void AddToggle(string t,bool initial,Action<bool> changed){Toggle x=new Toggle(t){value=initial};x.style.color=Paper;x.RegisterValueChangedCallback(e=>changed(e.newValue));content.Add(x);}
        private void AddSlider(string t,float min,float max,float initial,Action<float> changed){VisualElement row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.alignItems=Align.Center;content.Add(row);Label l=new Label(t);l.style.color=Paper;l.style.width=155;l.style.fontSize=10;row.Add(l);Slider s=new Slider(min,max){value=Mathf.Clamp(initial,min,max)};s.style.flexGrow=1;s.RegisterValueChangedCallback(e=>changed(e.newValue));row.Add(s);}
    }
}
