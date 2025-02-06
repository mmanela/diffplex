using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class DelimiterChunker : IChunker
#if !NET_TOO_OLD_VER
    , ISpanChunker
#endif
{
    private readonly char[] delimiters;

    public DelimiterChunker(char[] delimiters)
    {
        if (delimiters is null || delimiters.Length == 0)
        {
            throw new ArgumentException($"{nameof(delimiters)} cannot be null or empty.", nameof(delimiters));
        }

        this.delimiters = delimiters;
    }

    public IReadOnlyList<string> Chunk(string str)
    {
        var list = new List<string>();
        var begin = 0;
        var processingDelim = false;
        var delimBegin = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (Array.IndexOf(delimiters, str[i]) != -1)
            {
                if (i >= str.Length - 1)
                {
                    if (processingDelim)
                    {
                        list.Add(str.Substring(delimBegin, i + 1 - delimBegin));
                    }
                    else
                    {
                        list.Add(str.Substring(begin, i - begin));
                        list.Add(str.Substring(i, 1));
                    }
                }
                else
                {
                    if (!processingDelim)
                    {
                        // Add everything up to this delimeter as the next chunk (if there is anything)
                        if (i - begin > 0)
                        {
                            list.Add(str.Substring(begin, i - begin));
                        }

                        processingDelim = true;
                        delimBegin = i;
                    }
                }

                begin = i + 1;
            }
            else
            {
                if (processingDelim)
                {
                    if (i - delimBegin > 0)
                    {
                        list.Add(str.Substring(delimBegin, i - delimBegin));
                    }

                    processingDelim = false;
                }

                // If we are at the end, add the remaining as the last chunk
                if (i >= str.Length - 1)
                {
                    list.Add(str.Substring(begin, i + 1 - begin));
                }
            }
        }

        return list;
    }

#if !NET_TOO_OLD_VER
    public IReadOnlyList<string> Chunk(ReadOnlySpan<char> str)
    {
        var list = new List<string>();
        var begin = 0;
        var processingDelim = false;
        var delimBegin = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (Array.IndexOf(delimiters, str[i]) != -1)
            {
                if (i >= str.Length - 1)
                {
                    if (processingDelim)
                    {
                        list.Add(str.Slice(delimBegin, i + 1 - delimBegin).ToString());
                    }
                    else
                    {
                        list.Add(str.Slice(begin, i - begin).ToString());
                        list.Add(str.Slice(i, 1).ToString());
                    }
                }
                else
                {
                    if (!processingDelim)
                    {
                        // Add everything up to this delimeter as the next chunk (if there is anything)
                        if (i - begin > 0)
                        {
                            list.Add(str.Slice(begin, i - begin).ToString());
                        }

                        processingDelim = true;
                        delimBegin = i;
                    }
                }

                begin = i + 1;
            }
            else
            {
                if (processingDelim)
                {
                    if (i - delimBegin > 0)
                    {
                        list.Add(str.Slice(delimBegin, i - delimBegin).ToString());
                    }

                    processingDelim = false;
                }

                // If we are at the end, add the remaining as the last chunk
                if (i >= str.Length - 1)
                {
                    list.Add(str.Slice(begin, i + 1 - begin).ToString());
                }
            }
        }

        return list;
    }
#endif
}