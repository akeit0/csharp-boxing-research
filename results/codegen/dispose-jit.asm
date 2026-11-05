; Assembly listing for method DisposeResearch.Experiments.DisposeExperiments:Capture[DisposeResearch.Experiments.DefaultValue](byref):System.Object (FullOpts)
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
       mov      rcx, 0x7FFE32F7ADF0
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       mov      dword ptr [rax+0x08], ecx
       mov      rcx, rax
       call     [DisposeResearch.Experiments.IDefault:Capture():System.Object:this]
       nop      
 
G_M000_IG03:                ;; offset=0x0026
       add      rsp, 32
       pop      rbx
       ret      
 
; Total bytes of code 44

; Assembly listing for method DisposeResearch.CSharpDispose:Constrained[DisposeResearch.Experiments.DefaultValue](byref) (FullOpts)
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
       mov      rcx, 0x7FFE32F7ADF0
       call     CORINFO_HELP_NEWSFAST
       mov      ecx, dword ptr [rbx]
       mov      dword ptr [rax+0x08], ecx
       mov      rcx, rax
       call     [DisposeResearch.Experiments.IDefault:System.IDisposable.Dispose():this]
       nop      
 
G_M000_IG03:                ;; offset=0x0026
       add      rsp, 32
       pop      rbx
       ret      
 
; Total bytes of code 44

; Assembly listing for method DisposeResearch.CSharpDispose:Constrained[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40
 
G_M000_IG02:                ;; offset=0x0004
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop      
 
G_M000_IG03:                ;; offset=0x000B
       add      rsp, 40
       ret      
 
; Total bytes of code 16

; Assembly listing for method DisposeResearch.IlCalls:Dispose[DisposeResearch.Experiments.Counter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40
 
G_M000_IG02:                ;; offset=0x0004
       call     [DisposeResearch.Experiments.Counter:Dispose():this]
       nop      
 
G_M000_IG03:                ;; offset=0x000B
       add      rsp, 40
       ret      
 
; Total bytes of code 16

; Assembly listing for method DisposeResearch.IlCalls:Dispose[DisposeResearch.Experiments.ExplicitCounter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 1 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
 
G_M000_IG02:                ;; offset=0x0000
       inc      dword ptr [rcx]
 
G_M000_IG03:                ;; offset=0x0002
       ret      
 
; Total bytes of code 3

; Assembly listing for method DisposeResearch.IlCalls:Dispose[System.__Canon](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40
 
G_M000_IG02:                ;; offset=0x0004
       mov      rcx, gword ptr [rdx]
       mov      r11, 0x7FFE32C300F8
       call     [r11]System.IDisposable:Dispose():this
       nop      
 
G_M000_IG03:                ;; offset=0x0015
       add      rsp, 40
       ret      
 
; Total bytes of code 26

; Assembly listing for method DisposeResearch.IlCalls:Dispose[DisposeResearch.Experiments.ReferenceValue](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 1 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
 
G_M000_IG02:                ;; offset=0x0000
       inc      dword ptr [rcx+0x08]
       xor      rax, rax
       mov      gword ptr [rcx], rax
 
G_M000_IG03:                ;; offset=0x0008
       ret      
 
; Total bytes of code 9

; Assembly listing for method DisposeResearch.IlCalls:Dispose[DisposeResearch.Experiments.ThrowingCounter](byref) (FullOpts)
; Emitting BLENDED_CODE for generic X64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       sub      rsp, 40
 
G_M000_IG02:                ;; offset=0x0004
       call     [DisposeResearch.Experiments.ThrowingCounter:Dispose():this]
       int3     
 
; Total bytes of code 11

