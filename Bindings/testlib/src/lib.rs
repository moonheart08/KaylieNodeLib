#![feature(wasm_abi)]
#![feature(strict_provenance)]

use std::ptr;

#[no_mangle]
pub static mut SCREEN: [u32; 23040] = [0xFF0000FF; 23040];
#[no_mangle]
pub static mut TICK: u32 = 0;

#[no_mangle]
pub unsafe extern "wasm" fn worker()  {
    TICK += 1;
    let channel = ((TICK / 60) % 3) * 8;
    for x in 0..160 {
        for y in 0..144 {
            let color = (x ^ y) as u32;
            SCREEN[x + y * 144] = (color << channel) | 0xFF000000;
        }
    }
}
#[no_mangle]
pub extern "wasm" fn set_funny() -> i32 {
    unsafe {
        let ptr = 4 as *mut i32;
        ptr::write(ptr, 69);
    }
    return 1;
}
#[no_mangle]
pub extern "wasm" fn mirror() -> u64 {
    return unsafe { resonite_bindings::curtime() }
}

#[no_mangle]
pub extern "wasm" fn make_it_green() -> u32
{
    unsafe {
        for w in &mut SCREEN {
            *w = 0x00FF00FF;
        }

        return 0;
    }
}