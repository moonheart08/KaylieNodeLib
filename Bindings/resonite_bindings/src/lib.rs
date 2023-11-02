#![feature(wasm_abi)]
extern crate wee_alloc;

// Use `wee_alloc` as the global allocator.
#[global_allocator]
static ALLOC: wee_alloc::WeeAlloc = wee_alloc::WeeAlloc::INIT;

#[link(wasm_import_module = "env")]
extern "wasm" {
    pub fn abort(exitCode: i32);
    pub fn curtime() -> u64;
}