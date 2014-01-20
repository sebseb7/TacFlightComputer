/**
 * FlightComputerWindow.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class FlightComputerWindow : Window<FlightComputerWindow>
    {
        private readonly string version;
        private float lastUpdateTime = 0.0f;
        private float updateInterval = 0.1f;

        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private GUIStyle versionStyle;

        private string apoapsis = "";
        private string periapsis = "";
        private string timeToApoapsis = "";
        private string timeToPeriapsis = "";
        private string period = "";
        private string inclination = "";
        private string altitudeAboveSurface = "";
        private string biome = "";
        private string situation = "";

        private bool minimized = false;

        public FlightComputerWindow()
            : base("TAC Flight Computer", 200, 180)
        {
            base.HideCloseButton = true;
            version = Utilities.GetDllVersion(this);
        }

        public override ConfigNode Load(ConfigNode config)
        {
            ConfigNode windowConfig = base.Load(config);

            if (windowConfig != null)
            {
                minimized = Utilities.GetValue(windowConfig, "minimized", minimized);
            }

            return windowConfig;
        }

        public override ConfigNode Save(ConfigNode config)
        {
            ConfigNode windowConfig = base.Save(config);
            windowConfig.AddValue("minimized", minimized);
            return windowConfig;
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 1;
                labelStyle.wordWrap = false;

                valueStyle = new GUIStyle(labelStyle);
                valueStyle.alignment = TextAnchor.MiddleRight;
                valueStyle.stretchWidth = true;

                versionStyle = Utilities.GetVersionStyle();
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            if (!minimized)
            {
                float now = Time.time;
                if ((now - lastUpdateTime) > updateInterval)
                {
                    lastUpdateTime = now;
                    UpdateValues();
                }

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Apoapsis", labelStyle);
                GUILayout.Label("Periapsis", labelStyle);
                GUILayout.Label("Time to Apoapsis", labelStyle);
                GUILayout.Label("Time to Periapsis", labelStyle);
                GUILayout.Label("Period", labelStyle);
                GUILayout.Label("Inclination", labelStyle);
                GUILayout.Label("Altitude (surface)", labelStyle);
                GUILayout.Label("Biome", labelStyle);
                GUILayout.Label("Situation", labelStyle);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label(apoapsis, valueStyle);
                GUILayout.Label(periapsis, valueStyle);
                GUILayout.Label(timeToApoapsis, valueStyle);
                GUILayout.Label(timeToPeriapsis, valueStyle);
                GUILayout.Label(period, valueStyle);
                GUILayout.Label(inclination, valueStyle);
                GUILayout.Label(altitudeAboveSurface, valueStyle);
                GUILayout.Label(biome, valueStyle);
                GUILayout.Label(situation, valueStyle);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUI.Label(new Rect(4, windowPos.height - 13, windowPos.width - 20, 12), "TAC Flight Computer v" + version, versionStyle);
            }
            else
            {
                GUILayout.Space(1);
            }

            if (GUI.Button(new Rect(windowPos.width - 24, 4, 20, 20), "_", closeButtonStyle))
            {
                minimized = !minimized;
                windowPos.height = 10;
            }
        }

        private void UpdateValues()
        {
            if (FlightGlobals.ready && FlightGlobals.fetch.activeVessel != null)
            {
                Vessel vessel = FlightGlobals.fetch.activeVessel;
                Orbit orbit = vessel.orbit;

                apoapsis = FormatDistance(orbit.ApA);
                periapsis = FormatDistance(orbit.PeA);
                timeToApoapsis = Utilities.FormatTime(orbit.timeToAp, 1);
                timeToPeriapsis = Utilities.FormatTime(orbit.timeToPe, 1);
                period = Utilities.FormatTime(orbit.period, 1);
                inclination = orbit.inclination.ToString("0.00") + "°";

                if (vessel.terrainAltitude > 0)
                {
                    altitudeAboveSurface = FormatDistance(orbit.altitude - vessel.terrainAltitude);
                }
                else
                {
                    altitudeAboveSurface = FormatDistance(orbit.altitude);
                }

                CBAttributeMap.MapAttribute mapAttribute = vessel.mainBody.BiomeMap.GetAtt(Utilities.ToRadians(vessel.latitude), Utilities.ToRadians(vessel.longitude));
                biome = mapAttribute.name;

                situation = GetSituation(vessel);
            }
        }

        /*
         * Based on MechJebModuleInfoItems.CurrentBiome() from MechJeb:
         * https://github.com/MuMech/MechJeb2/blob/4d9365125df4934fa062817fef6732efa7b94780/MechJeb2/MechJebModuleInfoItems.cs#L951
         */
        private string GetSituation(Vessel vessel)
        {
            switch (vessel.situation)
            {
                case Vessel.Situations.PRELAUNCH:
                    return "Prelaunch";
                case Vessel.Situations.LANDED:
                    return "Landed";
                case Vessel.Situations.SPLASHED:
                    return "Splashed Down";
                case Vessel.Situations.FLYING:
                    if (vessel.altitude < vessel.mainBody.scienceValues.flyingAltitudeThreshold)
                    {
                        return "Flying";
                    }
                    else
                    {
                        return "Flying High";
                    }
                default:
                    if (vessel.altitude < vessel.mainBody.scienceValues.spaceAltitudeThreshold)
                    {
                        return "Near Space";
                    }
                    else
                    {
                        return "Space";
                    }
            }
        }

        private static string FormatDistance(double value)
        {
            string sign = "";
            if (value < 0.0)
            {
                sign = "-";
                value = -value;
            }

            if (value > 1000000.0)
            {
                return sign + (value / 1000000.0).ToString("0.00") + " Mm";
            }
            else if (value > 1000.0)
            {
                return sign + (value / 1000.0).ToString("0.00") + " km";
            }
            else
            {
                return sign + value.ToString("0.0") + " m";
            }
        }
    }
}
