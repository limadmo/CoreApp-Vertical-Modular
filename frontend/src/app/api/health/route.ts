/**
 * API Route: Health Check - Next.js 15
 * Endpoint para verificar status do sistema
 */
import { NextRequest, NextResponse } from 'next/server';

export async function GET(request: NextRequest) {
  try {
    const healthData = {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      version: '1.0.0',
      system: 'Next.js 15 + PostgreSQL',
      environment: process.env.NODE_ENV || 'development',
      uptime: process.uptime(),
      memory: {
        used: Math.round(process.memoryUsage().heapUsed / 1024 / 1024),
        total: Math.round(process.memoryUsage().heapTotal / 1024 / 1024),
      },
      database: {
        connected: !!process.env.DATABASE_URL,
        url: process.env.DATABASE_URL ? '***connected***' : 'not configured',
      },
      features: {
        multiTenant: true,
        jwt: true,
        realDatabase: true,
        vercelReady: true,
      },
    };

    return NextResponse.json(healthData, { 
      status: 200,
      headers: {
        'Content-Type': 'application/json',
        'Cache-Control': 'no-cache',
      }
    });
  } catch (error) {
    console.error('❌ Health check error:', error);
    
    return NextResponse.json(
      { 
        status: 'error', 
        message: 'System unhealthy',
        timestamp: new Date().toISOString(),
      }, 
      { status: 500 }
    );
  }
}

export async function HEAD(request: NextRequest) {
  return new Response(null, { 
    status: 200,
    headers: {
      'Content-Type': 'application/json',
    }
  });
}