///-----------------------------------------------------------------
///   File:     QuantImage.cs
///   Author:   Andre Laskawy           
///   Date:	    24.02.2018 15:35:57
///   Revision History: 
///   Name:  Andre Laskawy         Date:  24.02.2018 15:35:57      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common.Models
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the <see cref="JeckeImage" />
    /// </summary>
    public class AssassinImage
    {
        /// <summary>
        /// The image
        /// </summary>
        private byte[] compressedByteImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinImage"/> class.
        /// </summary>
        public AssassinImage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinImage"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="relation">The relation.</param>
        public AssassinImage(byte[] bytes, BaseModel relation)
        {
            this.Image = bytes;
            this.RelationId = relation.Id;
            this.RelatedTypeDescription = relation.TypeDiscriptor;
        }

        /// <summary>
        /// 
        /// Gets or sets the Id
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time.
        /// </summary>
        [JsonProperty(PropertyName = "CreatedDT")]
        public DateTime CreatedDT { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time.
        /// </summary>
        [JsonProperty(PropertyName = "ModifiedDT")]
        public DateTime ModifiedDT { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BaseModel" /> is synced.
        /// </summary>
        /// <value>
        ///   <c>true</c> if synced; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "Synced")]
        public bool Synced { get; set; }

        /// <summary>
        /// Gets or sets the relation identifier.
        /// </summary>
        [JsonProperty(PropertyName = "RelationId")]
        public Guid RelationId { get; set; }

        /// <summary>
        /// Gets or sets the related type description.
        /// </summary>
        [JsonProperty(PropertyName = "RelatedTypeDescription")]
        public string RelatedTypeDescription { get; set; }

        /// <summary>
        /// Gets or sets the image byte.
        /// </summary>
        [JsonProperty(PropertyName = "Image")]
        public byte[] Image
        {
            get
            {
                /*
                if (compressedByteImage != null)
                {
                    using (var compressedStream = new MemoryStream(compressedByteImage))
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
                */
                return compressedByteImage;
            }
            set
            {
                /*
                if (value != null)
                {
                    using (var compressedStream = new MemoryStream())
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        zipStream.Write(value, 0, value.Length);
                        zipStream.Close();
                        compressedByteImage = compressedStream.ToArray();
                    }
                }*/
                compressedByteImage = value;
            }
        }
    }
}
