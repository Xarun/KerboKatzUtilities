﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public partial class Toolbar : KerboKatzBase
  {
    public static List<string> toolbarOptions = new List<string> { "KerboKatz Toolbar", "Application Launcher" };
    private Toolbar()
    {
      modName = "KerboKatzToolbar";
      requiresUtilities = new Version(1, 2, 0);
      useToolbar = true;
    }

    protected override void Started()
    {
      setAppLauncherScenes(ApplicationLauncher.AppScenes.ALWAYS);
      currentSettings.load("", "KerboKatzToolbarSettings", modName);
      currentSettings.setDefault("UseBlizzyToolbar", "false");
      if (BlizzyToolbarManager.ToolbarAvailable)
      {
        toolbarOptions.AddUnique("Blizzy's toolbar");
      }
    }

    private static Dictionary<string, ToolbarClass> references = new Dictionary<string, ToolbarClass>();
    private List<ToolbarClass> visibleInThisScene = new List<ToolbarClass>();
    private static bool isUpdateRequired;
    private bool updateSingleIcon;

    public static void add(ToolbarClass toAdd)
    {
      if (toAdd.GameObject.modName == "KerboKatzToolbar")
        return;
      if (!references.ContainsKey(toAdd.GameObject.modName))
      {
        references.Add(toAdd.GameObject.modName, toAdd);
        isUpdateRequired = true;
      }
    }

    public static void remove(string modName)
    {
      if (references.ContainsKey(modName))
      {
        references.Remove(modName);
        isUpdateRequired = true;
      }
    }

    public static void changeIcon(string modName, Texture2D newIcon)
    {
      references[modName].icon = newIcon;
    }

    public bool isAnyActive()
    {
      if (isUpdateRequired)
      {
        updateVisibleList();
        isUpdateRequired = false;
      }
      if (visibleInThisScene.Count > 0)
      {
        return true;
      }
      return false;
    }

    private void updateVisibleList()
    {
      visibleInThisScene.Clear();
      foreach (var currentMod in references.Values)
      {
        if (isVisible(currentMod))
          visibleInThisScene.AddUnique(currentMod);
      }
    }

    public bool isVisible(ToolbarClass currentMod)
    {
      if (currentMod.onClick != null && currentMod.activeScences.Contains(HighLogic.LoadedScene))
        return true;
      return false;
    }

    private void Update()
    {
      if (isUpdateRequired)
      {
        if (isAnyActive())
        {
          if (visibleInThisScene.Count == 1)
          {
            setIcon(visibleInThisScene[0].icon);
            updateSingleIcon = true;
          }
          else
          {
            setIcon(Utilities.getTexture("KerboKatzToolbar", "Textures"));
            updateSingleIcon = false;
          }
          settingsWindowRect.height = 0;
        }
        else
        {
          updateSingleIcon = false;
        }
        if (visibleInThisScene.Count == 0)
        {
          useToolbar = true;
          removeFromBlizzyToolbar();
        }
        else
        {
          if (currentSettings.getBool("UseBlizzyToolbar"))
          {
            registerBlizzyToolbar();
          }
          else
          {
            useToolbar = false;
          }
          setAppLauncherScenes(ApplicationLauncher.AppScenes.ALWAYS);
        }
      }
      if (updateSingleIcon)
      {
        setIcon(visibleInThisScene[0].icon);
      }
    }

    protected override void onToolbar()
    {
      if (BlizzyToolbarManager.ToolbarAvailable && Input.GetMouseButtonUp(1) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
      {
        if (BlizzyToolbarButton == null)
        {
          currentSettings.set("UseBlizzyToolbar", true);
          registerBlizzyToolbar();
        }
        else
        {
          currentSettings.set("UseBlizzyToolbar", false);
          removeFromBlizzyToolbar();
          updateToolbarStatus();
        }
      }
      else if (visibleInThisScene.Count == 1)
      {
        visibleInThisScene[0].onClick();
      }
      else
      {
        if (currentSettings.getBool("showToolbar"))
        {
          currentSettings.set("showToolbar", false);
        }
        else
        {
          currentSettings.set("showToolbar", true);
        }
      }
    }

    protected override void OnDestroy()
    {
      if (currentSettings != null)
      {
        currentSettings.set("showToolbar", false);
        currentSettings.save();
      }
      removeFromBlizzyToolbar();
      removeFromToolbar();
      removeFromApplicationLauncher();
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
    }
  }

  public class ToolbarClass
  {
    public KerboKatzBase GameObject;
    public Action onClick;
    public Texture2D icon;
    public List<GameScenes> activeScences = new List<GameScenes>();
    public ToolbarClass(KerboKatzBase MonoBehaviour, Action onClick, Texture2D icon, params GameScenes[] GameScenes)
    {
      this.GameObject = MonoBehaviour;
      this.onClick = onClick;
      this.icon = icon;
      foreach (var c in GameScenes)
      {
        activeScences.Add(c);
      }
    }
  }
}