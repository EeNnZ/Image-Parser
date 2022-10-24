using ParserCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parsers.WallpaperAccess
{
    public class WallpaperAccessOptions : IParserOptions
    {
        public string BaseUrl => "https://wallpaperaccess.com/";

        public string SearchPrefix => "search?q=";

        #region Not used in case of this parser
        public string PagePrefix => "";
        public int StartPoint
        {
            get => -1;
            set => throw new NotImplementedException();
        }
        public int EndPoint
        {
            get => 0;
            set => throw new NotImplementedException();
        } 
        #endregion
        public string SearchQuery { get; set; } = "";
        public int ImageCount { get; set; }
        public string? Resolution { get; set; }
    }
}
