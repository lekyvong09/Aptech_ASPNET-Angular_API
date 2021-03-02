using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;

        public MessageRepository(DataContext context)
        {
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(messageParams => messageParams.MessageSent)
                .Include(s => s.Sender.Photos)
                .Include(r => r.Recipient.Photos)
                .AsSingleQuery()
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.Username == messageParams.Username),
                "Outbox" => query.Where(u => u.Sender.Username == messageParams.Username),
                _ => query.Where(u => u.Recipient.Username == messageParams.Username && u.DateRead == null)
            };

            var messages = query.Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderUsername = m.SenderUsername,
                    SenderPhotoUrl = m.Sender.Photos.FirstOrDefault(x => x.IsMain).Url,
                    RecipientId = m.RecipientId,
                    RecipientUsername = m.RecipientUsername,
                    RecipientPhotoUrl = m.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url,
                    Content = m.Content,
                    DateRead = m.DateRead,
                    MessageSent = m.MessageSent
            });

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public Task<IEnumerable<MessageDto>> GetMessageThread(int currentUserId, int recipientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
