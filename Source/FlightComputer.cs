/**
 * FlightComputer.cs
 * 
 * Thunder Aerospace Corporation's Flight Computer for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Apache License Version 2.0. See the LICENSE.txt and NOTICE.txt
 * files for more information.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TacFlightComputer : MonoBehaviour
    {
        private string configFilename;
        private FlightComputerWindow window;

        void Awake()
        {
            Debug.Log("TAC Flight Computer [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            configFilename = IOUtils.GetFilePathFor(this.GetType(), "FlightComputer.cfg");
            window = new FlightComputerWindow();
        }

        void Start()
        {
            Debug.Log("TAC Flight Computer [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
            Load();
            window.SetVisible(true);
        }

        void OnDestroy()
        {
            Debug.Log("TAC Flight Computer [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
            Save();
        }

        private void Load()
        {
            if (File.Exists<TacFlightComputer>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                window.Load(config);
            }
        }

        private void Save()
        {
            ConfigNode config = new ConfigNode();
            window.Save(config);

            config.Save(configFilename);
        }
    }
}
