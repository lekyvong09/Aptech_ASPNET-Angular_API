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
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender.Photos)
                .Include(u => u.Recipient.Photos)
                .Where(m => m.Recipient.Username == currentUsername
                        && m.Sender.Username == recipientUsername
                        || m.Recipient.Username == recipientUsername
                        && m.Sender.Username == currentUsername
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.Username == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var m in unreadMessages)
                {
                    m.DateRead = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
