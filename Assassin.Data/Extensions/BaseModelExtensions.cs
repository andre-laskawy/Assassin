///-----------------------------------------------------------------
///   File:     BaseModelExtensions.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 20:38:24
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 20:38:24      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common.Models
{
    using Assassin.Data.Api;
    using Assassin.Data.Context;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="BaseModelExtensions" />
    /// </summary>
    public static class BaseModelExtensions
    {
        /// <summary>
        /// Requests the image from server.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public static async Task RequestImageFromServer<T>(this T data, AssassinDataService service) where T : BaseModel
        {
            data = await service.QuantWebClient.GetImage(data);
        }

        /// <summary>
        /// Loads the images.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="refresh">if set to <c>true</c> [refresh].</param>
        public static bool LoadImages<T>(this T data, bool refresh = false) where T : BaseModel
        {
            try
            {
                foreach (var prop in data.GetType().GetProperties())
                {
                    if (prop.PropertyType == typeof(AssassinImage)
                        && (refresh || prop.GetValue(data) == null))
                    {
                        using (var ctx = new AssassinImageDataContext())
                        {
                            var image = ctx.Set<AssassinImage>().FirstOrDefault(p => p.RelationId == data.Id && p.RelatedTypeDescription == data.TypeDiscriptor);
                            if (image == null)
                            {
                                return false;
                            }

                            prop.SetValue(data, image);
                            ctx.DetachAllEntities();
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified BaseModel has image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>
        ///   <c>true</c> if the specified BaseModel has image; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasImage<T>(this T data) where T : BaseModel
        {
            foreach (var prop in data.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(AssassinImage))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static bool SetImage<T>(this T data, AssassinImage image) where T : BaseModel
        {
            foreach (var prop in data.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(AssassinImage))
                {
                    prop.SetValue(data, image);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static AssassinImage GetImage<T>(this T data) where T : BaseModel
        {
            foreach (var prop in data.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(AssassinImage))
                {
                    return prop.GetValue(data) as AssassinImage;
                }
            }

            return null;
        }

        public static void Save(this AssassinImage AssassinImage)
        {
            if (AssassinImage != null)
            {
                try
                {
                    using (var ctx = new AssassinImageDataContext())
                    {
                        if (!ctx.Set<AssassinImage>().Any(p => p.Id == AssassinImage.Id))
                        {
                            ctx.Set<AssassinImage>().Add(AssassinImage);
                        }
                        else
                        {
                            ctx.Set<AssassinImage>().Update(AssassinImage);
                        }

                        ctx.SaveChanges();
                        ctx.DetachAllEntities();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Deletes the specified quant image.
        /// </summary>
        /// <param name="AssassinImage">The quant image.</param>
        public static void Delete(this AssassinImage AssassinImage)
        {
            if (AssassinImage != null)
            {
                using (var ctx = new AssassinImageDataContext())
                {
                    if (ctx.Set<AssassinImage>().Any(p => p.Id == AssassinImage.Id))
                    {
                        ctx.Set<AssassinImage>().Remove(AssassinImage);
                        ctx.SaveChanges();
                        ctx.DetachAllEntities();
                    }
                }
            }
        }
    }
}
