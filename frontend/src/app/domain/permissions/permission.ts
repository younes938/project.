export const permissions = [
  'vehicle.process',
  'vehicle.inspect',
  'vehicle.search.recent',
  'vehicle.search.all',
  'vehicle.view',
  'vehicle.edit',
  'vehicle.cancel',
  'vehicle.export',
  'queue.view',
  'queue.history.view',
  'queue.pause',
  'queue.resume',
  'statistics.view',
  'persons.adjust',
] as const;

export type Permission = (typeof permissions)[number];
