using System.Globalization;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.Fake;

internal static class AdditionalRecordedForgeDatasets
{
    private static readonly RecordedEventTemplate[] TypeScriptEventTemplates =
    [
        Commit("microsoft/TypeScript", "1f70213d4922b434345f639b441681e470c7cfc1", "jakebailey", "Avoid async IPC panic on peer close (#64142)", "2026-09-04T21:35:32Z"),
        Commit("microsoft/TypeScript", "31e008726728d3c80d8dd5aea977dfe972dbf2b3", "copilot-swe-agent", "Prevent infinite loop in unstable AST JSDoc scanner (#64141)", "2026-09-04T21:30:59Z"),
        Commit("microsoft/TypeScript", "ab6a5a042860587381c0c45845f6e1ad787bf839", "Andarist", "Fix crash on private constructors in intersection base types (#64165)", "2026-09-04T19:37:19Z"),
        Commit("microsoft/TypeScript", "376f2731a445da264321158fbf964a84114f82d8", "ekalinin", "fix(checker): don't emit typeof for private-named static methods (#64…", "2026-09-04T19:22:18Z"),
        Commit("microsoft/TypeScript", "725f6124bf87595dc5e1557e83663ec96e8de06a", "weswigham", "Add pagination of batch requests (#64061)", "2026-09-04T18:25:48Z"),
        Commit("microsoft/TypeScript", "e6e89f9adfed350cdeb90ee19ce6c179f667ef74", "Andarist", "Handle tuple rest parameters in legacy decorator arity checks (#64164)", "2026-09-04T17:43:30Z"),
        Commit("microsoft/TypeScript", "45e8f87bff05fddd8839eef69df770f6aecfe7ec", "andrewbranch", "Decouple snapshot ownership from project.Session so api.Session only …", "2026-09-04T17:22:07Z"),
        Commit("microsoft/TypeScript", "b6634d86d232cc89706c07fc2fcebbce57e37eeb", "auvred", "Add auto-import retry to getCompletionsAtPosition in API (#64133)", "2026-09-04T16:28:42Z"),
        Commit("microsoft/TypeScript", "e4912cffa33aa800eba96e4c8a3245f80d8ce7a7", "luo2430", "Fix completions inside tuple types suggesting value symbols (#64146)", "2026-09-04T14:42:05Z"),
        Commit("microsoft/TypeScript", "e73c923cb58e9ea8cd75ba41c51b8d8886af3076", "oMatheusmol", "API: add getChildren and token getters to Node (#63893)", "2026-09-03T21:38:15Z"),
        Commit("microsoft/TypeScript", "387239768f4181244c9c357676fd3b47ed1eae7c", "weswigham", "Add tools/custom-gcl.exe.hash to .gitignore (#64156)", "2026-09-03T21:32:27Z"),
        Commit("microsoft/TypeScript", "0e32aa196a8f1814fb6a4fa294e2f49029d6de88", "camc314", "chore: remove `outFile`,`module:amd` config from test cases (#64122)", "2026-09-03T18:35:06Z"),
        Commit("microsoft/TypeScript", "cf7b9361a33fa0c8e1afa3cf45fde29c9ab23ec0", "Andarist", "Fixed `anyFunctionType` leak (#64131)", "2026-09-02T20:34:00Z"),
        Commit("microsoft/TypeScript", "f6b1667aa5c0468900eb2819ffcb41c0efd2cf09", "jakebailey", "Fix TestContentMapperOpenFileExcludedByConfigChange race (#64137)", "2026-09-02T17:43:19Z"),
        Commit("microsoft/TypeScript", "6ed67b6716f9992a8a30d51b69eb6844c9a9b173", "a-tarasyuk", "fix(62179): report non-string-literal values in import type attribute…", "2026-09-02T11:08:25Z"),
        Commit("microsoft/TypeScript", "43a90f4c105bc9db7cb7aa299beddafbabe1d23e", "weswigham", "Make the API disposable (#64117)", "2026-09-02T04:52:11Z"),
        Commit("microsoft/TypeScript", "11b6dbfab897a5796a75176d47ea90a67030d9a9", "jakebailey", "Bump and clean up deps, raise min local node version (#64054)", "2026-09-01T20:38:41Z"),
        Commit("microsoft/TypeScript", "949d81686f2e8fe1405d6d93b000b4a75b46b0d5", "youngspe", "Add diagnostic for private identifiers in destructuring patterns (#64…", "2026-09-01T20:02:21Z"),
        Commit("microsoft/TypeScript", "13e158b131a6ec523fc6ee76376c8ff1a55451ab", "copilot-swe-agent", "Clear stale incremental diagnostics after JSON module changes (#64026)", "2026-09-01T19:09:44Z"),
        Commit("microsoft/TypeScript", "473bcd20850bcc31c1a600b4427890c5ffef562a", "Andarist", "Keep instantiation expression symbols distinct and stable (#64123)", "2026-09-01T18:04:39Z")
    ];

    private static readonly RecordedEventTemplate[] CpythonEventTemplates =
    [
        Commit("python/cpython", "57594aae5e6155d30b9192a4cd85333584339445", "serhiy-storchaka", "gh-154511: Consolidate IDLE's mouse wheel handling in util (GH-156974)", "2026-09-06T06:15:08Z"),
        Commit("python/cpython", "7cd63cddade4c3e1de17c3443695afe1f41d8495", "J-M0", "no-issue: Add separate docstring for frozendict (gh-156986)", "2026-09-06T01:12:41Z"),
        Commit("python/cpython", "7a918411a300ddeef06d7f984bd23de8f6438421", "owenthcarey", "gh-127296: Document that argparse count action starts from a non-zero…", "2026-09-05T17:00:22Z"),
        Commit("python/cpython", "e208bf0dda73b8e5a32af154f448e68ad11b26b7", "deadlovelll", "gh-156988: Fix asyncio call graph breaking at sync generators (#156991)", "2026-09-05T16:48:38Z"),
        Commit("python/cpython", "14a93f4d91edf767e7b35413127cfcd6a2806983", "iritkatriel", "gh-156091: Fix crash compiling deeply nested inlined comprehensions (…", "2026-09-05T16:01:05Z"),
        Commit("python/cpython", "ba4a0790aa8438ccd847e86910f71c9823849d05", "tonghuaroot", "gh-156400: Close the socket or pipe when transport creation fails in …", "2026-09-05T15:46:11Z"),
        Commit("python/cpython", "e620377918c8927d98533bbfcca894c7e5e78158", "ByteFlowing1337", "gh-156970: Set `Mock.return_value`'s docstring (#156971)", "2026-09-05T14:07:19Z"),
        Commit("python/cpython", "5141621d44e8ee88d97a5b8b225c8a32e7a343f0", "vstinner", "gh-129813: Fix typo in byteswriter_resize() (#156938)", "2026-09-05T11:22:55Z"),
        Commit("python/cpython", "0ea2971c6eeaaed3429b5b655b77afe24fa798a8", "graingert", "gh-127057: reschedule proactor datagram read loop after ConnectionRes…", "2026-09-05T10:52:14Z"),
        Commit("python/cpython", "e56f86fb6cc7cf14c255d88465e9d51f6e977b4c", "iritkatriel", "gh-143493: fix cleanup on errors in codegen_comprehension (#156374)", "2026-09-04T17:15:44Z"),
        Commit("python/cpython", "3a7a22b5b0cf292ab9e7d046980085c227f7fdaa", "fedonman", "gh-156187: Fix the warning stacklevel inside a nested set operand (GH…", "2026-09-04T16:57:07Z"),
        Commit("python/cpython", "1a2e3a034df6074a900bd9f2cf23c5b164f5320b", "loic-simon", "gh-140870: Add PyREPL's module attributes import completion feature t…", "2026-09-04T14:58:12Z"),
        Commit("python/cpython", "1860130962b2fd92274a1c2bc74dd459313a2341", "StanFromIreland", "gh-156583: Remove unnecessary `test_sundry` (#156642)", "2026-09-04T14:44:09Z"),
        Commit("python/cpython", "bc977f450b328656e073b547f23aa201401023c7", "faithlesstomas", "gh-118150: difflib: expose autojunk flag from SequenceMatcher to publ…", "2026-09-04T14:20:30Z"),
        Commit("python/cpython", "aaae15c35b5fba1246b8998e6586a76fc1541be7", "serhiy-storchaka", "gh-155907: Fix error handling in the marshal C API (GH-155909)", "2026-09-04T13:44:57Z"),
        Commit("python/cpython", "7d71b3eae3cc297af3a55a30fccc85da5adeea7a", "encukou", "gh-97850: Add load_module() removal to 3.15 What's New (GH-156418)", "2026-09-04T13:21:01Z"),
        Commit("python/cpython", "2632610ec19123f7fb85effd632775011f92ab35", "encukou", "gh-142349: Clarify that sys.lazy_modules may contain extra items (GH-…", "2026-09-04T13:19:47Z"),
        Commit("python/cpython", "59a691361cb7b1d4fde3b92f98a61490381c62c9", "serhiy-storchaka", "gh-156894: Fix the position of syntax errors which cover a range (GH-…", "2026-09-04T11:14:13Z"),
        Commit("python/cpython", "e50d23389be07bf702bde74c3cda794c6febb53e", "terryjreedy", "gh-156896: Stop scrubbing tkinker submodules upon idlelib.run import …", "2026-09-04T11:10:07Z"),
        Commit("python/cpython", "112560ee34a70c02baebdc11519d1f77e90ecab8", "encukou", "gh-155561: Use multi-phase init for Modules/_testlimitedcapi.c (GH-15…", "2026-09-04T09:32:15Z")
    ];

    private static readonly RecordedEventTemplate[] OpenJdkEventTemplates =
    [
        Commit("openjdk/jdk", "ef54ca1c775881843f0710a8148f379a6e209816", "dcubed", "8391868: disable java/foreign/TestConfinedSegmentPoolDefensiveRelease…", "2026-09-05T01:07:18Z"),
        Commit("openjdk/jdk", "c144b2175d61a773f23c9f79cc5baa0f7e644dc0", "fmatte", "8391495: Apply JDK-8370201 to serviceability/sa/sadebugd/DebugdConnec…", "2026-09-04T19:18:08Z"),
        Commit("openjdk/jdk", "f1c7c3e9bc0c8794dbf16b4fdc31464d5abe473b", "mcimadamore", "8388329: Forward variable access in strict initializers bypass DA", "2026-09-04T17:17:50Z"),
        Commit("openjdk/jdk", "261cf7e32a09cbb9b207f89af3cd6e8d636d9c0d", "drwhite", "8389912: [REDO] Regression >6% on Crypto-MLDSA.sign on x64", "2026-09-04T15:43:26Z"),
        Commit("openjdk/jdk", "7f0c040b50d20433693fdfa175b81ebe500c183f", "kvn", "8391442: Test compiler/c1/CanonicalizeArrayLength.java is timing out …", "2026-09-04T15:28:32Z"),
        Commit("openjdk/jdk", "837596c39306d11365337c628294a8a4f19b107f", "minborg", "8391831: MemorySegment.asOverlappingSlice() fails for enclosed segment", "2026-09-04T15:00:43Z"),
        Commit("openjdk/jdk", "64dcc93f823dfd048b16edc4e8f84fce42dcb6bc", "tschatzl", "8391679: G1: Rename G1CSetCandidateGroup* to G1CardSetGroup*", "2026-09-04T13:07:41Z"),
        Commit("openjdk/jdk", "7a3d978495b5dde8b44e01eb64f5b5783cf982cb", "tschatzl", "8391753: G1: Fix groud-id typo in remembered set size message", "2026-09-04T13:07:08Z"),
        Commit("openjdk/jdk", "367e91379352eab9c6fbd1d24596fb0e5ac20575", "mhaessig", "8391808: Problemlist compiler/vectorapi/AllBitsSetVectorMatchRuleTest…", "2026-09-04T09:05:18Z"),
        Commit("openjdk/jdk", "094b580e1ac3281ef62d3aba41c1cb68caab9243", "fbredber", "8390970: Remove ObjectMonitor::_metadata", "2026-09-04T08:35:30Z"),
        Commit("openjdk/jdk", "7b9f8c7d60e5736a37aef1a3454c5bd4f08fc39b", "tstuefe", "8391169: (process) Stale comments after JDK-8357089", "2026-09-04T08:08:58Z"),
        Commit("openjdk/jdk", "4f61b215c35b40c35d34b78851328de8c6887a47", "johan-sjolen", "8385564: Replace VMA linked list with hashtable in MemBaseline", "2026-09-04T07:36:15Z"),
        Commit("openjdk/jdk", "f563cf09856563245a02ed11a56265597d214b24", "hgqxjj", "8389218: C2: assert(false) failed: PhaseCCP not at fixpoint: analysis…", "2026-09-04T07:32:09Z"),
        Commit("openjdk/jdk", "e3be6b303259ab194c39ff5fdc2471ded742814b", "MBaesken", "8391451: Various ErrorHandling - related hotspot tests fail after JDK…", "2026-09-04T07:30:07Z"),
        Commit("openjdk/jdk", "ad41115d1a1962dcddeb61a86705c8a22992130d", "mhaessig", "8385698: [Valhalla] C1 omits type profiling for null-free types", "2026-09-04T06:47:41Z"),
        Commit("openjdk/jdk", "1741a6391fad9887b0b5422f5859ae24be6dbb43", "kwei", "8391605: Record jfr stubs to aot code cache", "2026-09-04T06:47:19Z"),
        Commit("openjdk/jdk", "f0952f6ce6cc43db4326eda0e397711d36c31fc6", "jbhateja", "8382713: [VectorAPI] Perform late inlining of failed vector intrinsics", "2026-09-04T02:28:24Z"),
        Commit("openjdk/jdk", "5dc3e0e5dbc6842051c70b577cdf2cb314332eb7", "haosun", "8391599: Remove runtime/cds/appcds/aotCode/AOTCodeFlags.java from pro…", "2026-09-04T01:18:59Z"),
        Commit("openjdk/jdk", "8406c24c2c3c41cc1b71805c5e85c1407e2e5ace", "haosun", "8391600: [TESTBUG] Improve runtime/cds/TestDefaultArchiveLoading.java…", "2026-09-04T01:15:03Z"),
        Commit("openjdk/jdk", "23f4712d32e970bd0f90fac22f62c41d24dd5331", "YaSuenag", "8388696: DW_CFA_offset_extended and DW_CFA_restore_extended are not s…", "2026-09-04T00:01:38Z")
    ];

    private static readonly RecordedEventTemplate[] GoEventTemplates =
    [
        Commit("golang/go", "38d1265e1a015add1d0b8651f5c2ea3f06199765", "adonovan", "cmd/go: preserve vet export data in memory for dependent actions on f…", "2026-09-05T22:11:17Z"),
        Commit("golang/go", "c5941983810b68ba93c30f0ef22c91ad63fb3e5c", "cpu", "crypto/x509: reject empty GeneralSubtrees sequences", "2026-09-05T12:32:50Z"),
        Commit("golang/go", "98d17e5bf49c6342648ffea09699dfd4b4e38213", "cpu", "crypto/x509: update x509-limbo tests", "2026-09-05T12:32:44Z"),
        Commit("golang/go", "74cad37fc1c4214e821a98a84a36ed65c898fce4", "cpu", "crypto/x509: restrict URI constraints to exact hosts", "2026-09-05T12:32:38Z"),
        Commit("golang/go", "0b192682428e1c6366bdc4a1f47d0671e98cb25d", "cpu", "crypto/x509: restrict bare email constraints to exact hosts", "2026-09-05T12:32:30Z"),
        Commit("golang/go", "714d9afb66994e987aa715e088279e75efcec315", "dsnet", "encoding/json: avoid calling MarshalText on named string map keys", "2026-09-04T23:27:59Z"),
        Commit("golang/go", "0d1836382fd04a3cdfb967cc2ee3bba0cc7661ab", "bradfitz", "crypto/tls: don't pin record-sized buffers while blocked in Read", "2026-09-04T22:14:28Z"),
        Commit("golang/go", "78407c11223d3e388700adaed41d1ce3027ef246", "cuonglm", "cmd/compile: correctly mark interface calls as used for tail call wra…", "2026-09-04T22:09:50Z"),
        Commit("golang/go", "91ffdf91c1316c74ac93150c554738b7468e3be7", "tklauser", "net/http/internal/http2: use atomic.Bool for ClientConn.atomicReused", "2026-09-04T22:07:51Z"),
        Commit("golang/go", "6ff3155b9c568fced8168865c711c8ab76a4778f", "cuishuang", "unicode/utf16: avoid overallocating in Encode", "2026-09-04T22:07:38Z"),
        Commit("golang/go", "43bc472365b5cc9b8222c8b837abba7b0254af7b", "kanapitsas", "math: improve Log2 accuracy near 1", "2026-09-04T22:02:55Z"),
        Commit("golang/go", "cc740a9f1b3191174d3b7dda817e2daf07c78337", "TurboRx", "archive/tar: return ErrHeader on PAX UID/GID integer overflow", "2026-09-04T21:47:16Z"),
        Commit("golang/go", "c320fc145b4ad7ecd4a1b33822a81096c89dc452", "apocelipes", "internal/runtime,math,runtime: fix source file paths in comments", "2026-09-04T21:47:04Z"),
        Commit("golang/go", "ba7fdba26bb5f9698a0ccd719c49d5ba2d1327a2", "dr2chase", "simd/archsimd: emulate \"ReduceSum\" for wasm, arm64, amd64", "2026-09-04T20:52:58Z"),
        Commit("golang/go", "8b1c22e12f478d4dfc7f7a390e92fbc68085b87e", "JunyangShao", "simd/archsimd: test SVE ops across all element shapes", "2026-09-04T18:35:36Z"),
        Commit("golang/go", "76b21f0229db1156c7117a8e80879ce29474c052", "JunyangShao", "simd/archsimd: add SVE MulHigh", "2026-09-04T18:32:19Z"),
        Commit("golang/go", "cc71e4955cf1aea4299bfa3bc32b3c0e42327b31", "JunyangShao", "simd/archsimd: add SVE Mul", "2026-09-04T18:27:57Z"),
        Commit("golang/go", "70c31c1f4ff8ba16f85d4ab7787041cb072bf31e", "JunyangShao", "cmd/compile: include SVE predicate registers in arm64 callerSave", "2026-09-04T18:25:21Z"),
        Commit("golang/go", "7f9af814e191d4d5008b9c787b8f99fffbf1dd8f", "randall77", "cmd/compile: add SizeAndAlign support to ssa check mode", "2026-09-04T18:06:13Z"),
        Commit("golang/go", "2c31595783c74564c1299a5401dc6e606f379cc7", "bradfitz", "net/http/internal/http2: don't shrink HPACK decoder table before SETT…", "2026-09-03T18:40:30Z")
    ];

    private static readonly RecordedEventTemplate[] RustEventTemplates =
    [
        Commit("rust-lang/rust", "dbad1bab60868d49978b716635ac2c3203e327cf", "bors", "Auto merge of #162105 - Zalathar:arena-cache, r=nnethercote", "2026-09-06T02:32:06Z"),
        Commit("rust-lang/rust", "f248f4038796913873f11ca65b1b901e311c8dae", "bors", "Auto merge of #160524 - clubby789:opt-level-opt-none, r=RalfJung", "2026-09-05T20:19:46Z"),
        Commit("rust-lang/rust", "f207aa3913114c3326bc0b88acbd12b10244edf8", "bors", "Auto merge of #162333 - Zalathar:rollup-DdpfsFl, r=Zalathar", "2026-09-05T17:04:57Z"),
        Commit("rust-lang/rust", "be58a505e546602ab9faa0714dfbe2cf7e27a3a4", "Zalathar", "Remove an existing out-of-line impl of `Arena::alloc_os_str`", "2026-09-05T14:35:47Z"),
        Commit("rust-lang/rust", "9bc79fcda077bd512938c6258620c3657fe35c78", "Zalathar", "Remove some Copy types from the typed-arena list", "2026-09-05T14:35:47Z"),
        Commit("rust-lang/rust", "808023885b778340822dfaecf878c12578bd20dc", "Zalathar", "Use the dropless arena for query `crate_extern_paths`", "2026-09-05T14:35:47Z"),
        Commit("rust-lang/rust", "5303664f6ee9687004fbab79149d2e4f8598b205", "Zalathar", "Use the dropless arena for query `extra_filename`", "2026-09-05T14:35:47Z"),
        Commit("rust-lang/rust", "6ea33483a1da0c1dd4ea3910d8d2e61a39162379", "Zalathar", "Use the central arena for query `mir_shims`", "2026-09-05T14:35:46Z"),
        Commit("rust-lang/rust", "30c55b32d57e978cfb30a876e152c0a84e52975c", "Zalathar", "Use the central arena for query `mir_callgraph_cyclic`", "2026-09-05T14:35:46Z"),
        Commit("rust-lang/rust", "163ea8ee9324a20c37f8ba5ad32f9a3ac09db9bd", "Zalathar", "Use the central arena for query `index_ast`", "2026-09-05T14:35:44Z"),
        Commit("rust-lang/rust", "546f07f6ee18fa92e586e659ef888bbf0f99e6e7", "bors", "Auto merge of #162314 - weihanglo:update-cargo, r=weihanglo", "2026-09-05T13:52:50Z"),
        Commit("rust-lang/rust", "e9c2c8bcc2e9435f3339bec48400aec55864dd68", "Zalathar", "Rollup merge of #162318 - Zalathar:doc-rustc-open, r=Kobzol", "2026-09-05T13:04:53Z"),
        Commit("rust-lang/rust", "5fdb0883bdccccd24105676bbea93b6013a00c08", "Zalathar", "Rollup merge of #162265 - jnkel:cargotest-lockfile, r=jieyouxu", "2026-09-05T13:04:52Z"),
        Commit("rust-lang/rust", "c5fd862f9f084a2e256c65e519a278ec7f81d115", "Zalathar", "Rollup merge of #162250 - cuishuang:master, r=mu001999", "2026-09-05T13:04:52Z"),
        Commit("rust-lang/rust", "ecd550dd9984fd8528a0cc250f806351d1d71f1b", "Zalathar", "Rollup merge of #162248 - zakrad:regr-test-146084, r=Kivooeo", "2026-09-05T13:04:51Z"),
        Commit("rust-lang/rust", "bdedfdd5574b92dc564720e5fa6a82e412e46249", "Zalathar", "Rollup merge of #161616 - bardiharborow:get-unchecked-index-precondit…", "2026-09-05T13:04:50Z"),
        Commit("rust-lang/rust", "63fdaab13216ccf1a551a8dfda617b08f31c4c5f", "Zalathar", "Rollup merge of #161397 - Zalathar:tests-coverage, r=davidtwco", "2026-09-05T13:04:50Z"),
        Commit("rust-lang/rust", "eaa6c852f52368168aeca8211fffc1cac7b456d5", "Zalathar", "Rollup merge of #161940 - alexcrichton:wasip3, r=davidtwco", "2026-09-05T13:04:49Z"),
        Commit("rust-lang/rust", "b2fa8547ed2ce207015fc9b19e036a0e32229b5b", "Zalathar", "Rollup merge of #161895 - phlip9:phlip9/fix-sgx-alloc-align, r=JohnTitor", "2026-09-05T13:04:48Z"),
        Commit("rust-lang/rust", "b7cb7e039fae6affb3c50e76863ed7f048c7bac2", "Zalathar", "Rollup merge of #161263 - mejrs:expand, r=petrochenkov,JonathanBrouwer", "2026-09-05T13:04:48Z")
    ];

    private static readonly RecordedEventTemplate[] LlvmEventTemplates =
    [
        Commit("llvm/llvm-project", "25348f55f2c7befc9a8c262ae0a319cde32aa2e7", "arsenm", "CodeGen: Remove TargetOptions::NoTrappingFPMath (#221429)", "2026-09-06T08:01:50Z"),
        Commit("llvm/llvm-project", "cb25e7ca7cef9e84db077fe36f3d27b269394c65", "boomanaiden154", "[MLIR][Python] Pass -Wno-unused-template to nanobind build", "2026-09-06T06:52:03Z"),
        Commit("llvm/llvm-project", "a65eb8723eab7c35798a5349e8d739d2bb2c5ef8", "snarang181", "[mlir][scf] Fix WhileMoveIfDown with duplicated scf.condition operand…", "2026-09-06T06:49:59Z"),
        Commit("llvm/llvm-project", "509dc3826d40e931806f054725ed7bc0f3ba96af", "arsenm", "MC: Move BinutilsVersion from TargetOptions to MCTargetOptions (#221435)", "2026-09-06T06:39:24Z"),
        Commit("llvm/llvm-project", "116c6b570347e601bd0769b9354fca9f94f3a5fe", "tbaederr", "[clang][bytecode] Add missing condition scope to CXXForRangeStmt (#22…", "2026-09-06T06:35:25Z"),
        Commit("llvm/llvm-project", "89b97e94043b3a0d6e83d3607c00c14a089470c9", "arichardson", "[RISC-V] Use an optional offset for Xwch instructions", "2026-09-06T05:55:18Z"),
        Commit("llvm/llvm-project", "8e7fa6f756211dae0cc91221326faf1077e495b7", "arichardson", "[RISC-V] Use an optional offset for Xqc* instructions", "2026-09-06T05:52:11Z"),
        Commit("llvm/llvm-project", "e16901ce359a3162aa1c1d071a73392077e22744", "tbaederr", "[clang][bytecode] Use IsNonNull opcode in CXXNewExpr (#221515)", "2026-09-06T05:27:08Z"),
        Commit("llvm/llvm-project", "03ff69fd5f82a7607113b83e45bd3828842669e3", "tbaederr", "[clang][bytecode] Do pointer-to-bool convesions without classify() (#…", "2026-09-06T05:23:40Z"),
        Commit("llvm/llvm-project", "72417eb739e5d66fd345c3cc9e2265b8a7949fe1", "MaskRay", "[Xtensa] Depend on CodeGenTypes to fix BUILD_SHARED_LIBS=on build (#2…", "2026-09-06T03:43:10Z"),
        Commit("llvm/llvm-project", "bf085fa43725f4177f44b347aebeb69c96cb3800", "tbaederr", "[clang][bytecode] Cache Is(Unnamed)BitFields bools in `Record::Field`…", "2026-09-06T03:40:58Z"),
        Commit("llvm/llvm-project", "ad9259c18b15f1d2c35e2f8748afc7fd13c2e976", "dmaclach", "[include-cleaner] Add WalkAST unit tests for Objective-C constructs (…", "2026-09-06T02:27:43Z"),
        Commit("llvm/llvm-project", "012428c5e864d5e976638eb9e19ea207cd2cb829", "as4230", "[X86] Lower i64 vector division and remainder through float division …", "2026-09-06T01:27:06Z"),
        Commit("llvm/llvm-project", "62e6fe8bbab320f2517c004e6087be489f3ecb88", "vporpo", "Reapply \"[SandboxIR] Callback registration now allows specifying orde…", "2026-09-06T01:18:46Z"),
        Commit("llvm/llvm-project", "419c22c908c97dac45d161ce56fa01b99ec7debf", "MaskRay", "[TableGen] Unique the remaining Init pools in a UniquingSet. NFC (#22…", "2026-09-06T00:09:22Z"),
        Commit("llvm/llvm-project", "5115f32500a8977da758a268fc818872c6d4ee02", "fhahn", "[VPlan] Preserve branch weights from VPlan0 through to codegen. (#213…", "2026-09-05T21:29:42Z"),
        Commit("llvm/llvm-project", "94cb123a68a133ddcc1f7492a6a760f50c4d771e", "dmaclach", "[include-cleaner] Support Objective-C literals and boxed expressions …", "2026-09-05T21:24:14Z"),
        Commit("llvm/llvm-project", "f42c87ba2c70219b1f4ad1f031a5b6a1ea6603fb", "aobolensk", "[AMDGPU] Fix legacy fmin/fmax combines to preserve NaN and zero ties …", "2026-09-05T20:19:14Z"),
        Commit("llvm/llvm-project", "45b1e622ab5a917fcdb339c3f97dd64718b51341", "vkortbeek-gf", "[TypePromotion] Support trunc-to-i1 conditions (#216311)", "2026-09-05T20:06:37Z"),
        Commit("llvm/llvm-project", "03de3d1299bffd3a2829da92b28875142d8ad944", "boomanaiden154", "[Support] Fix -Wunused-template from debugString", "2026-09-05T19:56:31Z")
    ];

    public static RecordedForgeDataset Get(string dataset) => dataset switch
    {
        "microsoft-typescript" => new RecordedForgeDataset("microsoft/TypeScript", TypeScriptEventTemplates),
        "python-cpython" => new RecordedForgeDataset("python/cpython", CpythonEventTemplates),
        "openjdk-jdk" => new RecordedForgeDataset("openjdk/jdk", OpenJdkEventTemplates),
        "golang-go" => new RecordedForgeDataset("golang/go", GoEventTemplates),
        "rust-lang-rust" => new RecordedForgeDataset("rust-lang/rust", RustEventTemplates),
        "llvm-llvm-project" => new RecordedForgeDataset("llvm/llvm-project", LlvmEventTemplates),
        _ => throw new ArgumentException($"Unknown fake dataset '{dataset}'.", nameof(dataset))
    };

    private static RecordedEventTemplate Commit(
        string repository,
        string sha,
        string author,
        string summary,
        string createdAt) =>
        new(
            ForgeEventKind.Commit,
            author,
            summary,
            $"https://github.com/{repository}/commit/{sha}",
            DateTimeOffset.Parse(createdAt, CultureInfo.InvariantCulture),
            false);
}

internal sealed record RecordedForgeDataset(
    string Repository,
    IReadOnlyList<RecordedEventTemplate> Events);

internal sealed record RecordedEventTemplate(
    ForgeEventKind Kind,
    string Author,
    string Summary,
    string Url,
    DateTimeOffset CreatedAt,
    bool IsReply);
