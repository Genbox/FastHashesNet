﻿/*
*  xxHash - Fast Hash algorithm
*  Copyright (C) 2012-2016, Yann Collet
*
*  BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions are
*  met:
*
*  * Redistributions of source code must retain the above copyright
*  notice, this list of conditions and the following disclaimer.
*  * Redistributions in binary form must reproduce the above
*  copyright notice, this list of conditions and the following disclaimer
*  in the documentation and/or other materials provided with the
*  distribution.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
*  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
*  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
*  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
*  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
*  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
*  You can contact the author at :
*  - xxHash homepage: http://www.xxhash.com
*  - xxHash source repository : https://github.com/Cyan4973/xxHash
*/

//Ported to C# by Ian Qvist
//Source: http://cyan4973.github.io/xxHash/

using System.Runtime.CompilerServices;

namespace FastHashesNet.xxHash
{
    public static class xxHash64Unsafe
    {
        public static unsafe ulong ComputeHash(byte* data, int length, uint seed = 0)
        {
            uint bEnd = (uint)length;
            ulong h64;
            int offset = 0;

            if (length >= 32)
            {
                uint limit = bEnd - 32;
                ulong v1 = seed + xxHashConstants.PRIME64_1 + xxHashConstants.PRIME64_2;
                ulong v2 = seed + xxHashConstants.PRIME64_2;
                ulong v3 = seed + 0;
                ulong v4 = seed - xxHashConstants.PRIME64_1;

                do
                {
                    v1 = Round(v1, Utilities.Fetch64(data, offset));
                    offset += 8;
                    v2 = Round(v2, Utilities.Fetch64(data, offset));
                    offset += 8;
                    v3 = Round(v3, Utilities.Fetch64(data, offset));
                    offset += 8;
                    v4 = Round(v4, Utilities.Fetch64(data, offset));
                    offset += 8;
                } while (offset <= limit);

                h64 = Utilities.Rotate(v1, 1) + Utilities.Rotate(v2, 7) + Utilities.Rotate(v3, 12) + Utilities.Rotate(v4, 18);
                h64 = MergeRound(h64, v1);
                h64 = MergeRound(h64, v2);
                h64 = MergeRound(h64, v3);
                h64 = MergeRound(h64, v4);
            }
            else
            {
                h64 = seed + xxHashConstants.PRIME64_5;
            }

            h64 += (uint)length;

            while (offset + 8 <= bEnd)
            {
                ulong k1 = Round(0, Utilities.Fetch64(data, offset));
                h64 ^= k1;
                h64 = Utilities.Rotate(h64, 27) * xxHashConstants.PRIME64_1 + xxHashConstants.PRIME64_4;
                offset += 8;
            }

            if (offset + 4 <= bEnd)
            {
                h64 ^= Utilities.Fetch32(data, offset) * xxHashConstants.PRIME64_1;
                h64 = Utilities.Rotate(h64, 23) * xxHashConstants.PRIME64_2 + xxHashConstants.PRIME64_3;
                offset += 4;
            }

            while (offset < bEnd)
            {
                h64 ^= data[offset] * xxHashConstants.PRIME64_5;
                h64 = Utilities.Rotate(h64, 11) * xxHashConstants.PRIME64_1;
                offset++;
            }

            h64 ^= h64 >> 33;
            h64 *= xxHashConstants.PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= xxHashConstants.PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Round(ulong acc, ulong input)
        {
            acc += input * xxHashConstants.PRIME64_2;
            acc = Utilities.Rotate(acc, 31);
            acc *= xxHashConstants.PRIME64_1;
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MergeRound(ulong acc, ulong val)
        {
            val = Round(0, val);
            acc ^= val;
            acc = acc * xxHashConstants.PRIME64_1 + xxHashConstants.PRIME64_4;
            return acc;
        }
    }
}