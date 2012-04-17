open System
open System.IO
open System.Text.RegularExpressions

printfn @"#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007 Atif Aziz, Joseph Albahari. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// ""AS IS"" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion
"

let files = [
    "Enumerable.cs";
    "Enumerable.g.cs";
    "ExtensionAttribute.cs";
    "Func.cs";
    "IGrouping.cs";
    "ILookup.cs";
    "Internal.cs";
    "IOrderedEnumerable.cs";
    "Lookup.cs";
    "OrderedEnumerable.cs";
    "Action.cs" 
]

let headless text =
    let m = Regex.Match(text, @"^(?imsx: \s* \#region \b 
                                         .+? \#endregion \s*? $ \s* )", 
                              RegexOptions.CultureInvariant)
    if m.Success then true, text.Substring(m.Index + m.Length) else false, text

let dir =
    match fsi.CommandLineArgs |> List.ofArray with
    | _ :: []
        -> Directory.EnumerateFiles(Environment.CurrentDirectory, "Enumerable.cs", SearchOption.AllDirectories) 
           |> Seq.head 
           |> Path.GetDirectoryName
    | _ :: dir :: _
        -> dir
    | _ -> failwith "Unexpected usage!"

let sources = seq {
    for file in files do
        let path = Path.Combine(dir, file)
        let source = File.ReadAllText(path)
        yield match headless source with
              | true, source -> source.Trim() + Environment.NewLine
              | false, _     -> source
}

sources |> Seq.iter (printfn "%s")
