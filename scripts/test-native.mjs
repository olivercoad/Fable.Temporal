if (typeof globalThis.Temporal === 'undefined') {
  throw new Error('Native Temporal is required for this check. Use a Node release with native Temporal, such as Node 26.');
}
await import('../tests/dist/Tests.js');
