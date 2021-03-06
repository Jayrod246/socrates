﻿namespace Meyer.Socrates.Data
{
    using System;
    using Meyer.Socrates.Collections;
    using Meyer.Socrates.Data.Sections;
    using Meyer.Socrates.Data.Volatile;
    using System.Collections.Generic;
    using System.Linq;

    public class Chunk: VolatileObject, IChunkIdentifier, IResolvable<Section>, ICloneable
    {
        public uint ID { get => GetValue(ref id); set => SetValue(ref id, value); }
        public Quad Quad { get => GetValue(ref quad); set => SetValue(ref quad, value); }
        public ReferenceCollection References { get; }
        public IReadOnlyCollection<ReadOnlyReference> ReferencedBy => this.container?.GetReferencesTo(this) ?? Array.Empty<ReadOnlyReference>();
        public Section Section
        {
            get => GetValue(ref section);
            set
            {
                lock (Synchronized)
                {
                    if (value != null) value.owner = this;
                    if (section != null) section.owner = null;
                    SetValue(ref section, value);
                }
            }
        }
        public string String { get => GetValue(ref str); set => SetValue(ref str, value); }
        public ModeFlags Mode { get => GetValue(ref mode); set => SetValue(ref mode, value); }
        public uint SectionOffset => sectionOffset;

        public bool ForcesUnicode { get => GetValue(ref forcesUnicode); set => SetValue(ref forcesUnicode, value); }

        public Chunk()
        {
            References = new ReferenceCollection(this);
        }

        protected override bool SetValue<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            lock (Synchronized)
            {
                if (propertyName == nameof(Quad) || propertyName == nameof(ID))
                {
#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
                    CommonItemHelper.ValidateValue(this, ref container, ref field, value);
#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile
                }
                return base.SetValue(ref field, value, propertyName);
            }
        }

        protected override T GetValue<T>(ref T field, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            lock (Synchronized)
            {
                return base.GetValue(ref field, propertyName);
            }
        }

        internal volatile ChunkyCollection container;
        internal uint sectionOffset;

        private Section section;
        private uint id;
        private Quad quad;
        private ModeFlags mode;
        private string str;
        private bool forcesUnicode;

        Section IResolvable<Section>.Resolve()
        {
            return Section;
        }

        ChunkIdentity IChunkIdentifier.GetChunkIdentity()
        {
            return new ChunkIdentity(Quad, ID);
        }

        public Chunk Clone()
        {
            var copy = new Chunk()
            {
                Quad = Quad,
                ID = ID,
                Mode = Mode,
                String = String,
                Section = Section.Clone(),
                ForcesUnicode = ForcesUnicode
            };

            copy.References.AddRange(References.Select(r => r.Clone()));

            return copy;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
