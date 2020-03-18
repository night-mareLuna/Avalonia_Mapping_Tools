﻿using System.Collections.Generic;
using System.Text;

namespace Mapping_Tools.Classes.HitsoundStuff {

    /// <summary>
    /// 
    /// </summary>
    public class CustomIndex {

        /// <summary>
        /// 
        /// </summary>
        public int Index;
        
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, HashSet<SampleGeneratingArgs>> Samples;
        
        /// <summary>
        /// 
        /// </summary>
        public static readonly List<string> AllKeys = new List<string> { "normal-hitnormal", "normal-hitwhistle", "normal-hitfinish", "normal-hitclap",
                                                                         "soft-hitnormal", "soft-hitwhistle", "soft-hitfinish", "soft-hitclap",
                                                                         "drum-hitnormal", "drum-hitwhistle", "drum-hitfinish", "drum-hitclap" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public CustomIndex(int index) {
            Index = index;
            Samples = new Dictionary<string, HashSet<SampleGeneratingArgs>>();
            foreach (string key in AllKeys) {
                Samples[key] = new HashSet<SampleGeneratingArgs>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CustomIndex() {
            Index = -1;
            Samples = new Dictionary<string, HashSet<SampleGeneratingArgs>>();
            foreach (string key in AllKeys) {
                Samples[key] = new HashSet<SampleGeneratingArgs>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool CheckSupport(HashSet<SampleGeneratingArgs> s1, HashSet<SampleGeneratingArgs> s2) {
            // s2 fits in s1 or s2 is empty
            return s2.Count > 0 ? s1.SetEquals(s2) : true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool CheckCanSupport(HashSet<SampleGeneratingArgs> s1, HashSet<SampleGeneratingArgs> s2) {
            // s2 fits in s1 or s1 is empty or s2 is empty
            return s1.Count > 0 && s2.Count > 0 ? s1.SetEquals(s2) : true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Fits(CustomIndex other) {
            // Every non-empty set from other == set from self
            // True until false
            bool support = true;
            foreach (KeyValuePair<string, HashSet<SampleGeneratingArgs>> kvp in Samples) {
                support = CheckSupport(kvp.Value, other.Samples[kvp.Key]) && support; 
            }
            return support;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CanMerge(CustomIndex other) {
            // Every non-empty set from other == non-empty set from self
            // True until false
            bool support = true;
            foreach (KeyValuePair<string, HashSet<SampleGeneratingArgs>> kvp in Samples) {
                support = CheckCanSupport(kvp.Value, other.Samples[kvp.Key]) && support;
            }
            return support;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void MergeWith(CustomIndex other) {
            foreach (string key in AllKeys) {
                Samples[key].UnionWith(other.Samples[key]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public CustomIndex Merge(CustomIndex other) {
            CustomIndex ci = new CustomIndex();
            foreach (string key in AllKeys) {
                ci.Samples[key].UnionWith(other.Samples[key]);
            }
            return ci;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CustomIndex Copy() {
            CustomIndex ci = new CustomIndex(Index);
            ci.MergeWith(this);
            return ci;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadedSamples"></param>
        public void CleanInvalids(Dictionary<SampleGeneratingArgs, SampleSoundGenerator> loadedSamples = null) {
            // Replace all invalid paths with "" and remove the invalid path if another valid path is also in the hashset
            foreach (HashSet<SampleGeneratingArgs> paths in Samples.Values) {
                int initialCount = paths.Count;
                int removed = paths.RemoveWhere(o => !SampleImporter.ValidateSampleArgs(o, loadedSamples));

                if (paths.Count == 0 && initialCount != 0) {
                    // All the paths where invalid and it didn't just start out empty
                    paths.Add(new SampleGeneratingArgs());  // This "" is here to prevent this hashset from getting new paths
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var accumulator = new StringBuilder();
            foreach (KeyValuePair<string, HashSet<SampleGeneratingArgs>> kvp in Samples) {
                var sampleList = new StringBuilder();
                foreach (var sga in kvp.Value) {
                    sampleList.Append(string.Format("{0}|", sga.ToString()));
                }
                if (sampleList.Length > 0)
                    sampleList.Remove(sampleList.Length - 1, 1);
                accumulator.Append(string.Format("{0}: [{1}]", kvp.Key, sampleList.ToString()));
            }
            return accumulator.ToString();
        }
    }
}
