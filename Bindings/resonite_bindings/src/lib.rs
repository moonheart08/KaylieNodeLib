#![feature(wasm_abi)]
#![no_std]
extern crate wee_alloc;

use core::alloc::{GlobalAlloc, Layout};
use core::{mem, ptr};
use core::marker::PhantomData;
use core::ptr::slice_from_raw_parts;

#[global_allocator]
static ALLOC: wee_alloc::WeeAlloc = wee_alloc::WeeAlloc::INIT;

#[link(wasm_import_module = "env")]
extern "wasm" {
    pub fn abort(exitCode: i32);
    pub fn curtime() -> u64;
}

const ALIGNMENT: usize = 32;

#[no_mangle]
pub static __resonite_v1_str_format: i32 = 1;

#[no_mangle]
pub unsafe extern "wasm" fn __resonite_v1_alloc(size: i32) -> *mut u8 {
    let layout = Layout::from_size_align_unchecked((size + (mem::size_of::<i32>() as i32)) as usize, ALIGNMENT);
    let ptr = ALLOC.alloc(layout);
    ptr::write(ptr as *mut i32, size);
    return ptr.add(mem::size_of::<i32>());
}

#[no_mangle]
pub unsafe extern "wasm" fn __resonite_v1_dealloc(ptr: *mut u8) {
    let size = ptr::read((ptr.sub(mem::size_of::<i32>())) as *const i32);
    let layout = Layout::from_size_align_unchecked(size as usize, ALIGNMENT);
    ALLOC.dealloc(ptr, layout);
}

/// Webassembly argument ref.
#[repr(transparent)]
#[derive(Debug)]
pub struct WRef<T: ?Sized>
{
    inner: *const u8,
    _type: PhantomData<T>,
} 

impl<T: Sized> AsRef<T> for WRef<T>
{
    fn as_ref(&self) -> &T {
        unsafe { (self.inner as *const T).as_ref().unwrap() }
    }
}

impl<T> AsRef<[T]> for WRef<[T]> 
{
    fn as_ref(&self) -> &[T] {
        unsafe {
            let size = ptr::read(self.inner as *const i32);
            let offset_ptr = self.inner.add(mem::size_of::<i32>()) as *const T;
            return &*slice_from_raw_parts(offset_ptr, size as usize);
        }
    }
}

impl AsRef<str> for WRef<str> {
    fn as_ref(&self) -> &str {
        unsafe {
            //SAFETY: Same representation.
            let r = mem::transmute::<&WRef<str>, &WRef<[u8]>>(&self);
            //SAFETY: Once again, same representation.
            return mem::transmute::<&[u8], &str>(r.as_ref());
        }
    }
}

impl<T: ?Sized> Drop for WRef<T> {
    fn drop(&mut self) {
        if (self.inner as *const u8 as i32) < 0
        {
            return;
        }
        // We EXPLICITLY do not drop T!
        unsafe { __resonite_v1_dealloc(self.inner as *mut u8); }
    }
}

pub type WInt = i32;
pub type WLong = i64;
pub type WShort = i16;
pub type WSByte = i8;
pub type WUInt = u32;
pub type WULong = u64;
pub type WUShort = u16;
pub type WByte = u8;
pub type WFloat = f32;
pub type WDouble = f64;
pub type WFloatVec<const N: usize> = WRef<[f32; N]>;
pub type WFloat2 = WFloatVec<2>;
pub type WFloat3 = WFloatVec<3>;
pub type WFloat4 = WFloatVec<4>;
pub type WFloatMatrix<const W: usize, const H: usize> = WFloatVec<{ W * H }>;
pub type WFloat2x2 = WFloatMatrix<2, 2>;
pub type WFloat3x3 = WFloatMatrix<3, 3>;
pub type WFloat4x4 = WFloatMatrix<4, 4>;
pub type WDoubleVec<const N: usize> = WRef<[f64; N]>;
pub type WDouble2 = WDoubleVec<2>;
pub type WDouble3 = WDoubleVec<3>;
pub type WDouble4 = WDoubleVec<4>;
pub type WDoubleMatrix<const W: usize, const H: usize> = WDoubleVec<{ W * H }>;
pub type WDouble2x2 = WDoubleMatrix<2, 2>;
pub type WDouble3x3 = WDoubleMatrix<3, 3>;
pub type WDouble4x4 = WDoubleMatrix<4, 4>;
pub type WIntVec<const N: usize> = WRef<[i32; N]>;
pub type WInt2 = WIntVec<2>;
pub type WInt3 = WIntVec<3>;
pub type WInt4 = WIntVec<4>;
pub type WLongVec<const N: usize> = WRef<[i64; N]>;
pub type WLong2 = WLongVec<2>;
pub type WLong3 = WLongVec<3>;
pub type WLong4 = WLongVec<4>;
pub type WUIntVec<const N: usize> = WRef<[u32; N]>;
pub type WUInt2 = WUIntVec<2>;
pub type WUInt3 = WUIntVec<3>;
pub type WUInt4 = WUIntVec<4>;
pub type WULongVec<const N: usize> = WRef<[u64; N]>;
pub type WULong2 = WULongVec<2>;
pub type WULong3 = WULongVec<3>;
pub type WULong4 = WULongVec<4>;
pub type WStr = WRef<str>;
pub type WBinary = WRef<[u8]>;
pub type WBoolVec<const N: usize> = WRef<[bool; N]>;
pub type WBool2 = WBoolVec<2>;
pub type WBool3 = WBoolVec<3>;
pub type WBool4 = WBoolVec<4>;