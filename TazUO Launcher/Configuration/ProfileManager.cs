﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TazUO_Launcher
{
    static class ProfileManager
    {
        public static Task<Profile[]> GetAllProfiles()
        {
            return Task<Profile[]>.Factory.StartNew(() =>
            {
                if (Directory.Exists(LauncherSettings.ProfilesPath))
                {
                    string[] profiles = Directory.GetFiles(LauncherSettings.ProfilesPath, "*.json", SearchOption.TopDirectoryOnly);

                    List<Profile> list = new List<Profile>();

                    foreach (string profile in profiles)
                    {
                        try
                        {
                            var loadedProfile = JsonSerializer.Deserialize<Profile>(
                                File.ReadAllText(profile)
                                );
                            if (loadedProfile != null)
                            {
                                list.Add(loadedProfile);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"---- Error loading profile from [ {profile} ] ---");
                            Console.WriteLine(e.StackTrace);
                            Console.WriteLine();
                        }
                    }

                    if (list.Count == 0)
                    {
                        Profile blank = new Profile();
                        blank.Save();
                        list.Add(blank);
                    }

                    return list.ToArray();
                }
                else //Create new profile directory and a blank profile
                {
                    try
                    {
                        Directory.CreateDirectory(LauncherSettings.ProfilesPath);
                        Profile blank = new Profile();
                        blank.Save();
                        return new Profile[] { blank };

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("---- Error creating profile directory ---");
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine();
                    }
                    return Array.Empty<Profile>();
                }
            });
        }

        public static bool TryFindProfile(string? name, out Profile? profile)
        {
            if (name == null)
            {
                profile = null;
                return false;
            }

            Task<Profile[]> task = GetAllProfiles();
            task.Wait();

            foreach (Profile p in task.Result)
            {
                if (p.Name.Equals(name))
                {
                    profile = p;
                    return true;
                }
            }
            profile = null;
            return false;
        }

        public static void DeleteProfileFile(Profile profile, bool alsoDeleteSettingsFile = false)
        {
            try
            {
                if (File.Exists(profile.GetProfileFilePath()))
                {
                    File.Delete(profile.GetProfileFilePath());
                }

                if (alsoDeleteSettingsFile && File.Exists(profile.GetSettingsFilePath()))
                {
                    File.Delete(profile.GetSettingsFilePath());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("---- Error deleting profile ---");
                Console.WriteLine(e.ToString());
                Console.WriteLine();
            }
        }
    }
}
