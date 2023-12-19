using System;
using System.Text;
using Avalonia.Input;

namespace Mapping_Tools.Classes.SystemTools {
    public class Hotkey : ICloneable, IEquatable<Hotkey> {
        public Key Key { get; set; }

        public KeyModifiers Modifiers { get; set; }

        public Hotkey(Key key, KeyModifiers modifiers) {
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString() {
            var str = new StringBuilder();

            if (Modifiers.HasFlag(KeyModifiers.Control))
                str.Append("Ctrl + ");
            if (Modifiers.HasFlag(KeyModifiers.Shift))
                str.Append("Shift + ");
            if (Modifiers.HasFlag(KeyModifiers.Alt))
                str.Append("Alt + ");
            if (Modifiers.HasFlag(KeyModifiers.Meta))
                str.Append("Win + ");

            str.Append(Key);

            return str.ToString();
        }

        public object Clone() {
            return MemberwiseClone();
        }

        public bool Equals(Hotkey other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Hotkey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) Key * 397) ^ (int) Modifiers;
            }
        }
    }
}
