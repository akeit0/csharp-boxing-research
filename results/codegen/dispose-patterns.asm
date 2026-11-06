; CoreCLR runtime=.NET 10.0.14; arch=X64

; Assembly listing for method DisposeResearch.CSharpDispose:Pattern[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0008
       mov      rcx, 0x7FFE3228F058
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       add      rax, 8
       mov      dword ptr [rax], ecx
       mov      rcx, rax
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop

G_M000_IG03:                ;; offset=0x0029
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 47

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCast[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40

G_M000_IG02:                ;; offset=0x0004
       mov      ecx, dword ptr [rcx]
       mov      dword ptr [rsp+0x20], ecx
       lea      rcx, [rsp+0x20]
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop

G_M000_IG03:                ;; offset=0x0016
       add      rsp, 40
       ret

; Total bytes of code 27

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCastWithWriteBack[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx], bl
       mov      rcx, 0x7FFE3228F058
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       lea      rsi, bword ptr [rax+0x08]
       mov      dword ptr [rsi], ecx
       mov      rcx, rsi
       cmp      dword ptr [rcx], ecx
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       mov      eax, dword ptr [rsi]
       mov      dword ptr [rbx], eax

G_M000_IG03:                ;; offset=0x0031
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 56

; CoreCLR runtime=.NET 11.0.0-rc.3.26522.148; arch=X64

; Assembly listing for method DisposeResearch.CSharpDispose:Pattern[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0008
       mov      rcx, 0x7FFE5426A368
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       add      rax, 8
       mov      dword ptr [rax], ecx
       mov      rcx, rax
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop

G_M000_IG03:                ;; offset=0x0029
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 47

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCast[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40

G_M000_IG02:                ;; offset=0x0004
       mov      ecx, dword ptr [rcx]
       mov      dword ptr [rsp+0x20], ecx
       lea      rcx, [rsp+0x20]
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop

G_M000_IG03:                ;; offset=0x0016
       add      rsp, 40
       ret

; Total bytes of code 27

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCastWithWriteBack[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx], bl
       mov      rcx, 0x7FFE5426A368
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       lea      rsi, bword ptr [rax+0x08]
       mov      dword ptr [rsi], ecx
       mov      rcx, rsi
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       mov      eax, dword ptr [rsi]
       mov      dword ptr [rbx], eax

G_M000_IG03:                ;; offset=0x002F
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 54

; Native AOT runtime=.NET 10.0.14; arch=X64

; Assembly listing for method DisposeResearch.CSharpDispose:Pattern[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0008
       lea      rcx, [(reloc 0x424678)]
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       add      rax, 8
       mov      dword ptr [rax], ecx
       mov      rcx, rax
       call     DisposeResearch.Experiments.Counter:Dispose():this
       nop

G_M000_IG03:                ;; offset=0x0025
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 43

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCast[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40

G_M000_IG02:                ;; offset=0x0004
       mov      ecx, dword ptr [rcx]
       mov      dword ptr [rsp+0x20], ecx
       lea      rcx, [rsp+0x20]
       call     DisposeResearch.Experiments.Counter:Dispose():this
       nop

G_M000_IG03:                ;; offset=0x0015
       add      rsp, 40
       ret

; Total bytes of code 26

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCastWithWriteBack[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx], bl
       lea      rcx, [(reloc 0x420590)]
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       lea      rsi, bword ptr [rax+0x08]
       mov      dword ptr [rsi], ecx
       mov      rcx, rsi
       cmp      dword ptr [rcx], ecx
       call     DisposeResearch.Experiments.Counter:Dispose():this
       mov      eax, dword ptr [rsi]
       mov      dword ptr [rbx], eax

G_M000_IG03:                ;; offset=0x002D
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 52

; Native AOT runtime=.NET 11.0.0-rc.3.26522.148; arch=X64

; Assembly listing for method DisposeResearch.CSharpDispose:Pattern[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0008
       lea      rcx, [(reloc 0x4231d8)]
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       add      rax, 8
       mov      dword ptr [rax], ecx
       mov      rcx, rax
       call     DisposeResearch.Experiments.Counter:Dispose():this
       nop

G_M000_IG03:                ;; offset=0x0025
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 43

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCast[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40

G_M000_IG02:                ;; offset=0x0004
       mov      ecx, dword ptr [rcx]
       mov      dword ptr [rsp+0x20], ecx
       lea      rcx, [rsp+0x20]
       call     DisposeResearch.Experiments.Counter:Dispose():this
       nop

G_M000_IG03:                ;; offset=0x0015
       add      rsp, 40
       ret

; Total bytes of code 26

; Assembly listing for method DisposeResearch.CSharpDispose:PatternThenCastWithWriteBack[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for x64 on Windows
; FullOpts code
; NativeAOT compilation
; optimized code
; rsp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx], bl
       lea      rcx, [(reloc 0x423058)]
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       lea      rsi, bword ptr [rax+0x08]
       mov      dword ptr [rsi], ecx
       mov      rcx, rsi
       call     DisposeResearch.Experiments.Counter:Dispose():this
       mov      eax, dword ptr [rsi]
       mov      dword ptr [rbx], eax

G_M000_IG03:                ;; offset=0x002B
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 50
