# AGENTS.md

## Purpose
This project provides a **minimal Unity application** designed for simple PC benchmarking.  
The program does nothing except:
- Open a Unity window.
- Display an FPS counter.

It is intended as a **lightweight, silly, but effective way** to measure raw frame rates and system responsiveness in Unity with zero extra overhead.

---

## Agent: BareBench
**Name:** `BareBench`  
**Role:** Benchmark agent  
**Behavior:**  
- Measures real-time frames per second (FPS).  
- Displays instantaneous and smoothed FPS in the corner of the screen.  
- Disables VSync and frame caps by default (to expose maximum performance).  

---

## Features
- **Bare-bones UI**: Only an on-screen FPS counter.  
- **Headless Benchmarking**: Minimal assets, no gameplay, no extra rendering load.  
- **Cross-platform**: Works on hopefully anything
